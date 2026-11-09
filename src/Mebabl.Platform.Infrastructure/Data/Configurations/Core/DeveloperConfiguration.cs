using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class DeveloperConfiguration
    : IEntityTypeConfiguration<Developer>
{
    public void Configure(EntityTypeBuilder<Developer> builder)
    {
        builder.ToTable("Developers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique();

        builder.HasMany(x => x.Projects)
            .WithOne(x => x.Developer)
            .HasForeignKey(x => x.DeveloperId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.Developer)
            .HasForeignKey(x => x.DeveloperId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PasswordResetTokens)
            .WithOne(x => x.Developer)
            .HasForeignKey(x => x.DeveloperId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}