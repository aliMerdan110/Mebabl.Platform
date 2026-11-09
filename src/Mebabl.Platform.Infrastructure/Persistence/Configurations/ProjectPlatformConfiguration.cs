using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures a platform-specific application identity owned by a project.
/// A project may contain multiple applications for the same platform, but
/// an application identity must not be registered more than once in a project.
/// </summary>
public sealed class ProjectPlatformConfiguration
    : IEntityTypeConfiguration<ProjectPlatform>
{
    public void Configure(EntityTypeBuilder<ProjectPlatform> builder)
    {
        builder.ToTable(
            "ProjectPlatforms",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_ProjectPlatforms_Platform_NotBlank",
                    "length(trim(\"Platform\")) > 0");
            });

        builder.HasKey(x => x.Id);

        // Required by composite foreign keys from PlatformApplication and
        // other project-owned records. It also guarantees project consistency.
        builder.HasAlternateKey(x => new
        {
            x.ProjectId,
            x.Id
        });

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.Platform)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Nickname)
            .HasMaxLength(100);

        builder.Property(x => x.PackageName)
            .HasMaxLength(255);

        builder.Property(x => x.BundleId)
            .HasMaxLength(255);

        builder.Property(x => x.Domain)
            .HasMaxLength(255);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Main access path for listing a project's apps grouped by platform
        // and filtering active/inactive platform registrations.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Platform,
                x.IsActive
            })
            .HasDatabaseName("IX_ProjectPlatforms_Project_Platform_Active");

        // Package names, bundle IDs and domains identify app registrations.
        // Partial unique indexes ignore unset optional identifiers while
        // preventing duplicate identifiers inside the same project.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.PackageName
            })
            .IsUnique()
            .HasDatabaseName("UX_ProjectPlatforms_Project_PackageName")
            .HasFilter("\"PackageName\" IS NOT NULL AND \"PackageName\" <> ''");

        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.BundleId
            })
            .IsUnique()
            .HasDatabaseName("UX_ProjectPlatforms_Project_BundleId")
            .HasFilter("\"BundleId\" IS NOT NULL AND \"BundleId\" <> ''");

        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Domain
            })
            .IsUnique()
            .HasDatabaseName("UX_ProjectPlatforms_Project_Domain")
            .HasFilter("\"Domain\" IS NOT NULL AND \"Domain\" <> ''");

        // The Project -> Platforms relationship is configured centrally in
        // PlatformProjectConfiguration to keep each relationship configured once.
    }
}
