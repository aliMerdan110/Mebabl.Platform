using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Applications;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class ApplicationPlatformConfiguration
    : IEntityTypeConfiguration<ApplicationPlatform>
{
    public void Configure(
        EntityTypeBuilder<ApplicationPlatform> builder)
    {
        builder.ToTable("ApplicationPlatforms");

        builder.HasKey(x => x.Id);

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

        builder.HasOne(x => x.Application)
            .WithMany(x => x.Platforms)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.Platform
        })
        .IsUnique();
    }
}