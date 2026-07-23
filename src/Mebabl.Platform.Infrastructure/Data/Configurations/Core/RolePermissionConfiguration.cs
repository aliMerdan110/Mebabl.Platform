using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.RoleId,
            x.PermissionId
        })
        .IsUnique();
    }
}