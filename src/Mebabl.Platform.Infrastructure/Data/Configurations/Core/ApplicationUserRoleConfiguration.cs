using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("ApplicationUserRoles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.ApplicationUserId,
            x.RoleId
        })
        .IsUnique();
    }
}