using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class SecurityRuleConfiguration
    : IEntityTypeConfiguration<SecurityRule>
{
    public void Configure(
        EntityTypeBuilder<SecurityRule> builder)
    {
        builder.ToTable("SecurityRules");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.Permission)
            .HasMaxLength(100)
            .IsRequired();


        builder.Property(x => x.CanRead)
            .HasDefaultValue(true);

        builder.Property(x => x.CanWrite)
            .HasDefaultValue(false);

        builder.Property(x => x.CanDelete)
            .HasDefaultValue(false);

        builder.Property(x => x.CanQuery)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);


        builder.HasIndex(x => new
        {
            x.CollectionId,
            x.Permission
        })
        .IsUnique();


        builder.HasOne(x => x.Collection)
            .WithMany()
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}