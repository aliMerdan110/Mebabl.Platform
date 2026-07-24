using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        // استخدام مفتاح مركب لأن الكيان لا يملك Id
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        // العلاقات
        builder.HasOne(x => x.Role)
               .WithMany(r => r.RolePermissions)
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
               .WithMany(p => p.RolePermissions)
               .HasForeignKey(x => x.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}