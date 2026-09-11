using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations;

public sealed class ApplicationMobileAppLinkConfiguration
    : IEntityTypeConfiguration<ApplicationMobileAppLink>
{
    public void Configure(
        EntityTypeBuilder<ApplicationMobileAppLink> builder)
    {
        builder.ToTable("ApplicationMobileAppLinks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationId)
            .IsRequired();

        builder.Property(x => x.AndroidPackageName)
            .HasMaxLength(255);

        builder.Property(x => x.AndroidSha256CertificateFingerprint)
            .HasMaxLength(128);

        builder.Property(x => x.IosBundleId)
            .HasMaxLength(255);

        builder.Property(x => x.IosTeamId)
            .HasMaxLength(64);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ApplicationId);

        builder.HasOne<PlatformApplication>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}