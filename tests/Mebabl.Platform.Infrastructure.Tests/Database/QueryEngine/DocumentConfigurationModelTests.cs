using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Projects;
using Mebabl.Platform.Infrastructure.Tests.Support;
using Xunit;

namespace Mebabl.Platform.Infrastructure.Tests.Database.QueryEngine;

public sealed class DocumentConfigurationModelTests
{
    [Fact]
    public void Document_UsesProductionJsonbAndConcurrencyConfiguration()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var entity = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Document));

        Assert.NotNull(entity);
        Assert.Equal("Documents", entity!.GetTableName());
        Assert.Equal(
            "jsonb",
            entity.FindProperty(nameof(Document.Data))!.GetColumnType());
        Assert.True(
            entity.FindProperty(nameof(Document.Version))!.IsConcurrencyToken);

        var versionConstraint = Assert.Single(
            entity.GetCheckConstraints(),
            constraint => constraint.Name == "CK_Documents_Version_Positive");

        Assert.Contains("\"Version\" >= 1", versionConstraint.Sql);
    }

    [Fact]
    public void Document_UsesActiveKeyAndProjectCollectionIndexes()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var entity = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Document))!;
        var indexes = entity.GetIndexes().ToArray();

        var uniqueKeyIndex = Assert.Single(
            indexes,
            index => index.GetDatabaseName() ==
                     "UX_Documents_Project_Collection_Key_Active");

        Assert.True(uniqueKeyIndex.IsUnique);
        Assert.Equal("\"IsDeleted\" = false", uniqueKeyIndex.GetFilter());
        Assert.Equal(
            new[] { nameof(Document.ProjectId), nameof(Document.CollectionId), nameof(Document.Key) },
            uniqueKeyIndex.Properties.Select(property => property.Name));

        var chronologyIndex = Assert.Single(
            indexes,
            index => index.GetDatabaseName() ==
                     "IX_Documents_Project_Collection_CreatedAt_Id_Active");

        Assert.Equal("\"IsDeleted\" = false", chronologyIndex.GetFilter());
        Assert.Equal(
            new[]
            {
                nameof(Document.ProjectId),
                nameof(Document.CollectionId),
                nameof(Document.CreatedAt),
                nameof(Document.Id)
            },
            chronologyIndex.Properties.Select(property => property.Name));
    }

    [Fact]
    public void Document_UsesProjectAndCompositeCollectionForeignKeys()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var entity = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Document))!;
        var foreignKeys = entity.GetForeignKeys().ToArray();

        var projectForeignKey = Assert.Single(
            foreignKeys,
            foreignKey => foreignKey.PrincipalEntityType.ClrType ==
                          typeof(PlatformProject));

        Assert.Equal(
            new[] { nameof(Document.ProjectId) },
            projectForeignKey.Properties.Select(property => property.Name));
        Assert.Equal(DeleteBehavior.Restrict, projectForeignKey.DeleteBehavior);

        var collectionForeignKey = Assert.Single(
            foreignKeys,
            foreignKey => foreignKey.PrincipalEntityType.ClrType ==
                          typeof(Collection));

        Assert.Equal(
            new[] { nameof(Document.ProjectId), nameof(Document.CollectionId) },
            collectionForeignKey.Properties.Select(property => property.Name));
        Assert.Equal(
            new[] { nameof(Collection.ProjectId), nameof(Collection.Id) },
            collectionForeignKey.PrincipalKey.Properties.Select(property => property.Name));
        Assert.Equal(DeleteBehavior.Restrict, collectionForeignKey.DeleteBehavior);
    }

    [Fact]
    public void Document_ConstrainsCreatorApplicationToTheSameProject()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var entity = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Document))!;

        var applicationForeignKey = Assert.Single(
            entity.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType ==
                          typeof(PlatformApplication));

        Assert.Equal(
            new[]
            {
                nameof(Document.ProjectId),
                nameof(Document.CreatedByApplicationId)
            },
            applicationForeignKey.Properties.Select(property => property.Name));
        Assert.Equal(
            new[] { nameof(PlatformApplication.ProjectId), nameof(PlatformApplication.Id) },
            applicationForeignKey.PrincipalKey.Properties.Select(property => property.Name));
        Assert.Equal(DeleteBehavior.Restrict, applicationForeignKey.DeleteBehavior);
    }
}
