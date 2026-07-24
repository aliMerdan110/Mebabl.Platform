using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationConfiguration 
    : IEntityTypeConfiguration<Mebabl.Platform.Domain.Entities.Application>
{
    public void Configure(
        EntityTypeBuilder<Mebabl.Platform.Domain.Entities.Application> builder)
    {

         // أضف هذه الأسطر داخل دالة Configure
builder.HasIndex(x => x.Code).IsUnique();
builder.Property(x => x.Code).IsRequired().HasMaxLength(50);

builder.HasOne(x => x.Tenant)
       .WithMany(t => t.Applications)
       .HasForeignKey(x => x.TenantId)
       .OnDelete(DeleteBehavior.Cascade);
       
        builder.ToTable("Applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}