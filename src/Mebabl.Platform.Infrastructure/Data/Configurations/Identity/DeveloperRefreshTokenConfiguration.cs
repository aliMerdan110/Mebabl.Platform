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

        builder.Property(x => x.Token)
            .IsRequired();

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasOne(x => x.Developer)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}