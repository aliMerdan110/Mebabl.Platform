using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

/// <summary>
/// Configures an application as a platform client registered inside a project.
/// The project is the data-ownership boundary; ApplicationId identifies the
/// client application and must never replace ProjectId in data isolation.
/// </summary>
public sealed class PlatformApplicationConfiguration
    : IEntityTypeConfiguration<PlatformApplication>
{
    public void Configure(EntityTypeBuilder<PlatformApplication> builder)
    {
        builder.ToTable(
            "Applications",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Applications_Name_Not_Empty",
                    "length(btrim(\"Name\")) > 0");

                table.HasCheckConstraint(
                    "CK_Applications_Code_Not_Empty",
                    "length(btrim(\"Code\")) > 0");
            });

        builder.HasKey(x => x.Id);

        // Required as a principal key for project-consistent references,
        // including documents that record their creating application.
        builder.HasAlternateKey(x => new
            {
                x.ProjectId,
                x.Id
            })
            .HasName("AK_Applications_ProjectId_Id");

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.ProjectPlatformId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Application codes are unique only within their owning project.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Code
            })
            .IsUnique()
            .HasDatabaseName("UX_Applications_ProjectId_Code");

        // Supports project application listing and active/inactive filtering.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.IsActive
            })
            .HasDatabaseName("IX_Applications_ProjectId_IsActive");

        // Supports grouping applications by their registered platform.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.ProjectPlatformId
            })
            .HasDatabaseName("IX_Applications_ProjectId_ProjectPlatformId");

        builder.HasOne(x => x.Project)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // The composite FK prevents an application from pointing to a
        // platform registration belonging to a different project.
        builder.HasOne(x => x.ProjectPlatform)
            .WithMany()
            .HasForeignKey(x => new
            {
                x.ProjectId,
                x.ProjectPlatformId
            })
            .HasPrincipalKey(x => new
            {
                x.ProjectId,
                x.Id
            })
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AuthProviders)
            .WithOne(x => x.Application)
            .HasForeignKey(x => x.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Credentials)
            .WithOne(x => x.Application)
            .HasForeignKey(x => x.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // User records are retained until an explicit account/application
        // lifecycle operation handles their dependent identity data.
        builder.HasMany(x => x.Users)
            .WithOne(x => x.Application)
            .HasForeignKey(x => x.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Roles)
            .WithOne(x => x.Application)
            .HasForeignKey(x => x.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.Application)
            .HasForeignKey(x => x.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
