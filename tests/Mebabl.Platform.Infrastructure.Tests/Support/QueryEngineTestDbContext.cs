using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Projects;
using Mebabl.Platform.Infrastructure.Persistence.Configurations;

namespace Mebabl.Platform.Infrastructure.Tests.Support;

/// <summary>
/// Uses the production CollectionConfiguration and DocumentConfiguration
/// so PostgreSQL query translation tests share the actual project-scoped
/// collection/document mappings and relationships.
/// </summary>
internal sealed class QueryEngineTestDbContext(
    DbContextOptions<QueryEngineTestDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Only principal key declarations are needed for the referenced
        // project and application entities in this focused query-engine model.
        modelBuilder.Entity<PlatformProject>(entity =>
        {
            entity.HasKey(project => project.Id);
        });

        modelBuilder.Entity<PlatformApplication>(entity =>
        {
            entity.HasKey(application => application.Id);
        });

        // Apply the real production mappings. DocumentConfiguration adds the
        // { ProjectId, Id } alternate key on Collection that its composite FK
        // uses to guarantee same-project ownership.
        modelBuilder.ApplyConfiguration(new CollectionConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
    }
}
