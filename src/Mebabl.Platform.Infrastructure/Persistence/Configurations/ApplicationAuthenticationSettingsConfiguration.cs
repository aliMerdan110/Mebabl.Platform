using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class ApplicationAuthenticationSettingsConfiguration
    : IEntityTypeConfiguration<ApplicationAuthenticationSettings>
{
    public void Configure(
        EntityTypeBuilder<ApplicationAuthenticationSettings> builder)
    {
        builder.ToTable("ApplicationAuthenticationSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationId)
            .IsRequired();

        builder.Property(x => x.AllowRegistration)
            .IsRequired();

        builder.Property(x => x.RequireEmailVerification)
            .IsRequired();

        builder.Property(x => x.AllowPasswordAuthentication)
            .IsRequired();

        builder.Property(x => x.AllowAnonymousAuthentication)
            .IsRequired();

        builder.Property(x => x.PasswordMinLength)
            .IsRequired();

        builder.Property(x => x.SessionLifetimeDays)
            .IsRequired();

        builder.Property(x => x.RefreshTokenLifetimeDays)
            .IsRequired();

        builder.Property(x => x.MaxLoginAttempts)
            .IsRequired();

        builder.HasIndex(x => x.ApplicationId)
            .IsUnique();

        builder.HasOne(x => x.Application)
            .WithOne()
            .HasForeignKey<ApplicationAuthenticationSettings>(
                x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}