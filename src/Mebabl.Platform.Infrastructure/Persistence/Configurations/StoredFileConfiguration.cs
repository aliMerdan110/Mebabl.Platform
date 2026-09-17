using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Infrastructure.Data.Configurations;

public sealed class StoredFileConfiguration
    : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(
        EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("StoredFiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired(false);

        builder.Property(x => x.Path)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Extension)
            .HasMaxLength(50);

        builder.Property(x => x.StorageKey)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Hash)
            .HasMaxLength(128);

        builder.Property(x => x.CacheControl)
            .HasMaxLength(500);

        builder.Property(x => x.ContentDisposition)
            .HasMaxLength(500);

        builder.Property(x => x.Size)
            .IsRequired();

        builder.Property(x => x.IsPublic)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.HasIndex(x =>
            new
            {
                x.ApplicationId,
                x.StorageKey
            })
            .IsUnique();

        builder.HasIndex(x =>
            new
            {
                x.ApplicationId,
                x.UserId,
                x.Path
            });
    }
}