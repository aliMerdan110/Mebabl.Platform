using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationId)
            .IsRequired();

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedUsername)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.SecurityStamp)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.EmailConfirmed)
            .IsRequired();

        builder.Property(x => x.TwoFactorEnabled)
            .IsRequired();

        builder.Property(x => x.LockoutEnabled)
            .IsRequired();

        builder.Property(x => x.AccessFailedCount)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.NormalizedEmail
        }).IsUnique();

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.NormalizedUsername
        }).IsUnique();

        builder.HasOne(x => x.Account)
            .WithMany(x => x.ApplicationUsers)
            .HasForeignKey(x => x.AccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Application)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PasswordResetTokens)
            .WithOne(x => x.ApplicationUser)
            .HasForeignKey(x => x.ApplicationUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.EmailVerificationTokens)
            .WithOne(x => x.ApplicationUser)
            .HasForeignKey(x => x.ApplicationUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.ApplicationUser)
            .HasForeignKey(x => x.ApplicationUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.ApplicationUser)
            .HasForeignKey(x => x.ApplicationUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}