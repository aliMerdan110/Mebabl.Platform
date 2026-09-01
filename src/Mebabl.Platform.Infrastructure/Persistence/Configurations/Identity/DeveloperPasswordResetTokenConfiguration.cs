
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Identity;

public sealed class DeveloperPasswordResetTokenConfiguration
    : IEntityTypeConfiguration<DeveloperPasswordResetToken>
{
    public void Configure(
        EntityTypeBuilder<DeveloperPasswordResetToken> builder)
    {
        builder.ToTable("DeveloperPasswordResetTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.UsedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Developer)
            .WithMany(x => x.PasswordResetTokens)
            .HasForeignKey(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.DeveloperId,
            x.ExpiresAt
        });
    }
}