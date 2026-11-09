using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class ApplicationUserRoleConfiguration
    : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("ApplicationUserRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ApplicationUserId,
            x.RoleId
        }).IsUnique();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.ApplicationUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}