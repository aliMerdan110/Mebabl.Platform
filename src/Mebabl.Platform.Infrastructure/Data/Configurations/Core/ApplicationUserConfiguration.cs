using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(x => x.Id);

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

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Account)
            .WithMany(a => a.ApplicationUsers)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Application)
            .WithMany(a => a.Users)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}