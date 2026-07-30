using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Token)
               .IsUnique();

        builder.Property(x => x.Token)
               .IsRequired()
               .HasMaxLength(512);

        builder.Property(x => x.ExpiresAt)
               .IsRequired();

        builder.HasOne(x => x.ApplicationUser)
               .WithMany(x => x.RefreshTokens)
               .HasForeignKey(x => x.ApplicationUserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}