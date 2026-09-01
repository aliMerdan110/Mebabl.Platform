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

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.EmailVerificationTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}