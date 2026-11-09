using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Projects;
using Mebabl.Platform.Infrastructure.Persistence.Configurations;

namespace Mebabl.Platform.Infrastructure.Tests.Support;

/// <summary>
/// Minimal PostgreSQL model for executing the production batch handler against
/// real transactions. Collection and Document mappings come from production.
/// </summary>
internal sealed class BatchWriteTestDbContext(
    DbContextOptions<BatchWriteTestDbContext> options) : DbContext(options)
{
    public DbSet<PlatformProject> Projects => Set<PlatformProject>();
    public DbSet<PlatformApplication> Applications => Set<PlatformApplication>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlatformProject>(builder =>
        {
            builder.ToTable("Projects");
            builder.HasKey(project => project.Id);
            builder.Ignore(project => project.Developer);
            builder.Ignore(project => project.Platforms);
            builder.Ignore(project => project.Applications);
            builder.Ignore(project => project.SdkConfiguration);
        });

        modelBuilder.Entity<PlatformApplication>(builder =>
        {
            builder.ToTable("Applications");
            builder.HasKey(application => application.Id);
            builder.HasAlternateKey(application => new
            {
                application.ProjectId,
                application.Id
            });

            builder.Ignore(application => application.Project);
            builder.Ignore(application => application.ProjectPlatform);
            builder.Ignore(application => application.AuthProviders);
            builder.Ignore(application => application.Credentials);
            builder.Ignore(application => application.Users);
            builder.Ignore(application => application.Roles);
            builder.Ignore(application => application.Permissions);
        });

        modelBuilder.ApplyConfiguration(new CollectionConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
    }
}
