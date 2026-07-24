using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username).IsRequired().HasMaxLength(256);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Bio).HasMaxLength(500);
        builder.Property(x => x.AvatarUrl).HasMaxLength(1000);
        
        // تم تعريف العلاقة مسبقاً في AccountConfiguration، ولكن نؤكد الفهرس هنا
        builder.HasIndex(x => x.AccountId).IsUnique(); 
    }
}