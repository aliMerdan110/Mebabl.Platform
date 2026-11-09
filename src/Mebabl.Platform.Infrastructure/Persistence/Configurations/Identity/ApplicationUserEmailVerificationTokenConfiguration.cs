using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Identity;

public sealed class ApplicationUserEmailVerificationTokenConfiguration
    : IEntityTypeConfiguration<ApplicationUserEmailVerificationToken>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUserEmailVerificationToken> builder)
    {
        builder.ToTable("ApplicationUserEmailVerificationTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.UsedAt);

        builder.HasOne(x => x.ApplicationUser)
            .WithMany(x => x.EmailVerificationTokens)
            .HasForeignKey(x => x.ApplicationUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.ApplicationUserId,
            x.ExpiresAt
        });
    }
}