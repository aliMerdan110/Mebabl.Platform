using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Commerce;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Commerce;

public sealed class ProductImageConfiguration
    : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StorageFileId)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.Order
        });

        builder.HasIndex(x => x.StorageFileId);
    }
}