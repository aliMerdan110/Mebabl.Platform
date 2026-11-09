using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        // Legacy token storage; nullable during migration.
        builder.Property(x => x.Token)
            .HasMaxLength(512)
            .IsRequired(false);

        // SHA-256 hash of the refresh token.
        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .IsRequired(false);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.ApplicationUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}