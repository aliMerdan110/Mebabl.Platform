using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Projects;
using Mebabl.Platform.Infrastructure.Tests.Support;
using Xunit;

namespace Mebabl.Platform.Infrastructure.Tests.Database.BatchWrites;

public sealed class BatchWriteDocumentsPostgreSqlIntegrationTests
{
    [PostgreSqlFact]
    public async Task Batch_CommitsCreateUpdateAndDeleteTogether()
    {
        await using var database = await PostgreSqlBatchWriteDatabase.CreateAsync();
        var seed = await SeedAsync(database, includeSecondDocument: true);

        using var createData = JsonDocument.Parse("""{"kind":"created"}""");
        using var updateData = JsonDocument.Parse("""{"kind":"updated"}""");

        var command = new BatchWriteDocumentsCommand(
            seed.CollectionId,
            new BatchWriteDocumentOperation[]
            {
                new(BatchWriteOperationType.Create, "created", createData.RootElement.Clone()),
                new(
                    BatchWriteOperationType.Update,
                    "updated",
                    updateData.RootElement.Clone(),
                    seed.FirstDocumentId,
                    1),
                new(
                    BatchWriteOperationType.Delete,
                    DocumentId: seed.SecondDocumentId,
                    ExpectedVersion: 1)
            });

        await using (var context = database.CreateContext())
        {
            var response = await CreateHandler(context, seed).Handle(
                command,
                CancellationToken.None);

            Assert.Equal(seed.CollectionId, response.CollectionId);
            Assert.Equal(3, response.Results.Count);
            Assert.Equal(0, response.Results[0].Index);
            Assert.Equal(BatchWriteOperationType.Create, response.Results[0].Type);
            Assert.Equal(1, response.Results[0].Version);
            Assert.Equal(1, response.Results[1].Index);
            Assert.Equal(2, response.Results[1].Version);
            Assert.Equal(2, response.Results[2].Index);
            Assert.True(response.Results[2].IsDeleted);
        }

        await using var verification = database.CreateContext();
        var documents = await verification.Documents
            .AsNoTracking()
            .OrderBy(document => document.Key)
            .ToListAsync();

        Assert.Equal(3, documents.Count);

        var updated = Assert.Single(documents, document => document.Id == seed.FirstDocumentId);
        Assert.Equal("updated", updated.Key);
        Assert.Equal(2, updated.Version);
        Assert.Equal("updated", updated.Data.RootElement.GetProperty("kind").GetString());

        var deleted = Assert.Single(documents, document => document.Id == seed.SecondDocumentId);
        Assert.True(deleted.IsDeleted);
        Assert.Equal(2, deleted.Version);

        var created = Assert.Single(documents, document => document.Key == "created");
        Assert.Equal(1, created.Version);
        Assert.False(created.IsDeleted);
    }

    [PostgreSqlFact]
    public async Task Batch_RollsBackEarlierMutationsWhenDatabaseRejectsALaterMutation()
    {
        await using var database = await PostgreSqlBatchWriteDatabase.CreateAsync();
        var seed = await SeedAsync(database, includeSecondDocument: false);
        await database.InstallFailureTriggerAsync();

        using var updateData = JsonDocument.Parse("""{"kind":"should-rollback"}""");
        using var createData = JsonDocument.Parse("""{"kind":"failure"}""");

        var command = new BatchWriteDocumentsCommand(
            seed.CollectionId,
            new BatchWriteDocumentOperation[]
            {
                new(
                    BatchWriteOperationType.Update,
                    "should-rollback",
                    updateData.RootElement.Clone(),
                    seed.FirstDocumentId,
                    1),
                new(
                    BatchWriteOperationType.Create,
                    "db-failure",
                    createData.RootElement.Clone())
            });

        await using (var context = database.CreateContext())
        {
            var handler = CreateHandler(context, seed);
            await Assert.ThrowsAsync<DbUpdateException>(
                () => handler.Handle(command, CancellationToken.None));
        }

        await using var verification = database.CreateContext();
        var original = await verification.Documents
            .AsNoTracking()
            .SingleAsync(document => document.Id == seed.FirstDocumentId);

        Assert.Equal("original", original.Key);
        Assert.Equal(1, original.Version);
        Assert.Equal("original", original.Data.RootElement.GetProperty("kind").GetString());
        Assert.False(await verification.Documents.AnyAsync(document => document.Key == "db-failure"));
    }

    private static async Task<SeedInfo> SeedAsync(
        PostgreSqlBatchWriteDatabase database,
        bool includeSecondDocument)
    {
        var projectId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var firstDocumentId = Guid.NewGuid();
        var secondDocumentId = includeSecondDocument ? Guid.NewGuid() : Guid.Empty;
        var now = DateTime.UtcNow;

        await using var context = database.CreateContext();

        var project = new PlatformProject
        {
            Id = projectId,
            DeveloperId = Guid.NewGuid(),
            Name = "Batch integration project",
            Code = $"batch-{Guid.NewGuid():N}"[..20],
            Description = "Isolated integration-test project",
            IsActive = true,
            CreatedAt = now
        };

        var application = new PlatformApplication
        {
            Id = applicationId,
            ProjectId = projectId,
            ProjectPlatformId = Guid.NewGuid(),
            Name = "Batch integration app",
            Code = $"app-{Guid.NewGuid():N}"[..20],
            Description = "Isolated integration-test application",
            IsActive = true,
            CreatedAt = now
        };

        var collection = new Collection
        {
            Id = collectionId,
            ProjectId = projectId,
            Project = project,
            Name = "Batch documents",
            Code = "batch_documents",
            Description = "Batch-write integration tests",
            IsSystem = false,
            IsActive = true,
            CreatedAt = now
        };

        context.Projects.Add(project);
        context.Applications.Add(application);
        context.Collections.Add(collection);

        using var originalData = JsonDocument.Parse("""{"kind":"original"}""");
        context.Documents.Add(new Document
        {
            Id = firstDocumentId,
            ProjectId = projectId,
            Project = project,
            CollectionId = collectionId,
            Collection = collection,
            CreatedByApplicationId = applicationId,
            CreatedByUserId = Guid.NewGuid(),
            Key = "original",
            Data = JsonDocument.Parse(originalData.RootElement.GetRawText()),
            Version = 1,
            IsDeleted = false,
            CreatedAt = now
        });

        if (includeSecondDocument)
        {
            context.Documents.Add(new Document
            {
                Id = secondDocumentId,
                ProjectId = projectId,
                Project = project,
                CollectionId = collectionId,
                Collection = collection,
                CreatedByApplicationId = applicationId,
                CreatedByUserId = Guid.NewGuid(),
                Key = "to-delete",
                Data = JsonDocument.Parse("""{"kind":"to-delete"}"""),
                Version = 1,
                IsDeleted = false,
                CreatedAt = now
            });
        }

        await context.SaveChangesAsync();
        return new SeedInfo(projectId, applicationId, collectionId, firstDocumentId, secondDocumentId);
    }

    private static BatchWriteDocumentsCommandHandler CreateHandler(
        BatchWriteTestDbContext context,
        SeedInfo seed)
    {
        var dbContext = DispatchProxy.Create<
            IApplicationDbContext,
            ApplicationDbContextDispatchProxy>();
        ((ApplicationDbContextDispatchProxy)(object)dbContext).Initialize(context);

        var currentUser = new IntegrationCurrentUser(
            seed.ProjectId,
            seed.ApplicationId);

        return new BatchWriteDocumentsCommandHandler(
            dbContext,
            currentUser,
            new AllowAllDocumentSecurityService(),
            new IntegrationClock());
    }

    private sealed record SeedInfo(
        Guid ProjectId,
        Guid ApplicationId,
        Guid CollectionId,
        Guid FirstDocumentId,
        Guid SecondDocumentId);

    private sealed class IntegrationCurrentUser(Guid projectId, Guid applicationId) : ICurrentUser
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid AccountId { get; } = Guid.NewGuid();
        public Guid ApplicationId { get; } = applicationId;
        public Guid ProjectId { get; } = projectId;
        public bool IsAuthenticated => true;
    }

    private sealed class IntegrationClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class AllowAllDocumentSecurityService : IDocumentSecurityService
    {
        public Task EnsureReadAsync(Guid collectionId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnsureWriteAsync(Guid collectionId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnsureDeleteAsync(Guid collectionId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnsureQueryAsync(Guid collectionId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlySet<Guid>> GetReadableCollectionIdsAsync(
            IReadOnlyCollection<Guid> collectionIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlySet<Guid>>(collectionIds.ToHashSet());
    }
}
