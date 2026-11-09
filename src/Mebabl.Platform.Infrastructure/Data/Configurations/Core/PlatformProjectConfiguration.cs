using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class PlatformProjectConfiguration
    : IEntityTypeConfiguration<PlatformProject>
{
    public void Configure(EntityTypeBuilder<PlatformProject> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeveloperId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.DeveloperId,
            x.Code
        }).IsUnique();

        builder.HasOne(x => x.Developer)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.DeveloperId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Platforms)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // The one-to-one SDK configuration relationship is configured
        // exclusively in ProjectSdkConfigurationConfiguration.
    }
}