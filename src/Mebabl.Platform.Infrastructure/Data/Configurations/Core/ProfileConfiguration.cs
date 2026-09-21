
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class ProfileConfiguration
    : IEntityTypeConfiguration<Profile>
{
    public void Configure(
        EntityTypeBuilder<Profile> builder)
    {
        // يعرّف ملف الحساب وعلاقته الفريدة بحساب واحد.
        builder.ToTable("Profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Bio)
            .HasMaxLength(500);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.AccountId)
            .IsUnique();

        builder.HasOne(x => x.Account)
            .WithOne(x => x.Profile)
            .HasForeignKey<Profile>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
