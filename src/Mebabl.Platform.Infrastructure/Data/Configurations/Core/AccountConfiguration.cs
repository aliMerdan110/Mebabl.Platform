using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(x => x.Id);

        // الفهارس لمنع التكرار وتسريع البحث
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.HasIndex(x => x.NormalizedUsername).IsUnique();
        builder.HasIndex(x => x.TenantId);

        // القيود
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.NormalizedEmail).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(256);
        builder.Property(x => x.NormalizedUsername).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.SecurityStamp).IsRequired().HasMaxLength(256);

        // العلاقات
        builder.HasOne(x => x.Tenant)
               .WithMany(t => t.Accounts)
               .HasForeignKey(x => x.TenantId)
               .OnDelete(DeleteBehavior.Restrict); // منع حذف المستأجر إذا كان لديه حسابات

        builder.HasOne(x => x.Profile)
               .WithOne(p => p.Account)
               .HasForeignKey<Profile>(p => p.AccountId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}