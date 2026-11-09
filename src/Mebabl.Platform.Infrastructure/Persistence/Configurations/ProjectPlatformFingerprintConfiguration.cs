using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class ProjectPlatformFingerprintConfiguration
    : IEntityTypeConfiguration<ProjectPlatformFingerprint>
{
    public void Configure(
        EntityTypeBuilder<ProjectPlatformFingerprint> builder)
    {
        builder.ToTable("ProjectPlatformFingerprints");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectPlatformId)
            .IsRequired();

        builder.Property(x => x.FingerprintType)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(95)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProjectPlatformId,
            x.FingerprintType,
            x.Value
        }).IsUnique();

        builder.HasOne(x => x.ProjectPlatform)
            .WithMany()
            .HasForeignKey(x => x.ProjectPlatformId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}