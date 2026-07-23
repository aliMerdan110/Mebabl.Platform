using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationUserConfiguration 
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}