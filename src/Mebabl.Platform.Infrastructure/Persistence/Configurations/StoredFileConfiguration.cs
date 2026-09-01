using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("StoredFiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(150);

        builder.Property(x => x.Extension)
            .HasMaxLength(20);

        builder.Property(x => x.Hash)
            .HasMaxLength(128);

        builder.Property(x => x.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");

        builder.Property(x => x.Version)
            .HasDefaultValue(1);

        builder.HasIndex(x => new
        {
            x.BucketId,
            x.Key
        }).IsUnique();

        builder.HasOne(x => x.Bucket)
            .WithMany(x => x.Files)
            .HasForeignKey(x => x.BucketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}