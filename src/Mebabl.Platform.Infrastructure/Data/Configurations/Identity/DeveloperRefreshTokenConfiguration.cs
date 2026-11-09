using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Identity;

public sealed class DeveloperRefreshTokenConfiguration
    : IEntityTypeConfiguration<DeveloperRefreshToken>
{
    public void Configure(
        EntityTypeBuilder<DeveloperRefreshToken> builder)
    {
        builder.ToTable("DeveloperRefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeveloperId)
            .IsRequired();

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

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.DeveloperId,
            x.ExpiresAt
        });

        builder.HasOne(x => x.Developer)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.DeveloperId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}