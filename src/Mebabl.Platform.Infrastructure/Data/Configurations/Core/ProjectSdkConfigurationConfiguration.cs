using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class ProjectSdkConfigurationConfiguration
    : IEntityTypeConfiguration<ProjectSdkConfiguration>
{
    public void Configure(
        EntityTypeBuilder<ProjectSdkConfiguration> builder)
    {
        builder.ToTable("ProjectSdkConfigurations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.PublicKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ConfigurationVersion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.ProjectId)
            .IsUnique();

        builder.HasIndex(x => x.ProjectNumber)
            .IsUnique();

        builder.HasOne(x => x.Project)
            .WithOne(x => x.SdkConfiguration)
            .HasForeignKey<ProjectSdkConfiguration>(
                x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}