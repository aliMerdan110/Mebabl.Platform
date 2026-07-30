using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(x => x.Id);

        // منع تكرار نفس الحساب في نفس التطبيق
        builder.HasIndex(x => new { x.AccountId, x.ApplicationId }).IsUnique();

        builder.Property(x => x.CreatedAt).IsRequired();

        // العلاقات
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
