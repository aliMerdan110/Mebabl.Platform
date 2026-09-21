using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class ApplicationUserRoleConfiguration
    : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUserRole> builder)
    {
        // يمنع تكرار إسناد الدور نفسه للمستخدم.
        builder.ToTable("ApplicationUserRoles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.ApplicationUserId,
            x.RoleId
        })
        .IsUnique();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}