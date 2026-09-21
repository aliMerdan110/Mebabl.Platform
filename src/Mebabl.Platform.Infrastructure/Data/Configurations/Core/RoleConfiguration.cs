using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class RoleConfiguration
    : IEntityTypeConfiguration<Role>
{
    public void Configure(
        EntityTypeBuilder<Role> builder)
    {
        // يضمن تفرد الأدوار داخل كل تطبيق.
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.Code
        })
        .IsUnique();

        // builder.HasOne(x => x.Application)
        //     .WithMany()
        //     .HasForeignKey(x => x.ApplicationId)
        //     .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Application)
    .WithMany(x => x.Roles)
    .HasForeignKey(x => x.ApplicationId)
    .OnDelete(DeleteBehavior.Cascade);
    }

}