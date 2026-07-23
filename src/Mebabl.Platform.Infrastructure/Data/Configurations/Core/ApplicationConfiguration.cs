using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationConfiguration 
    : IEntityTypeConfiguration<Mebabl.Platform.Domain.Entities.Application>
{
    public void Configure(
        EntityTypeBuilder<Mebabl.Platform.Domain.Entities.Application> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}