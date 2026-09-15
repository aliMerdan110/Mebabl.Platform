
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Storage;

public sealed class StoredFileConfiguration
: IEntityTypeConfiguration<StoredFile>
{
public void Configure(EntityTypeBuilder<StoredFile> builder)
{
builder.ToTable("StoredFiles");


    builder.HasKey(x => x.Id);

    builder.Property(x => x.ApplicationId)
        .IsRequired();

    builder.Property(x => x.UserId)
        .IsRequired(false);

    builder.Property(x => x.Name)
        .HasMaxLength(255)
        .IsRequired();

    builder.Property(x => x.Path)
        .HasMaxLength(1024)
        .IsRequired();

    builder.Property(x => x.FileName)
        .HasMaxLength(255)
        .IsRequired();

    builder.Property(x => x.ContentType)
        .HasMaxLength(255)
        .IsRequired();

    builder.Property(x => x.Extension)
        .HasMaxLength(32)
        .IsRequired();

    builder.Property(x => x.Size)
        .IsRequired();

    builder.Property(x => x.Hash)
        .HasMaxLength(128)
        .IsRequired(false);

    builder.Property(x => x.StorageKey)
        .HasMaxLength(2048)
        .IsRequired();

    builder.Property(x => x.CacheControl)
        .HasMaxLength(512)
        .IsRequired(false);

    builder.Property(x => x.ContentDisposition)
        .HasMaxLength(512)
        .IsRequired(false);

    builder.Property(x => x.Version)
        .HasDefaultValue(1)
        .IsRequired();

    builder.Property(x => x.IsPublic)
        .HasDefaultValue(false)
        .IsRequired();

    builder.Property(x => x.IsDeleted)
        .HasDefaultValue(false)
        .IsRequired();

    builder.HasIndex(x => new
    {
        x.ApplicationId,
        x.Path
    });

    builder.HasIndex(x => new
    {
        x.ApplicationId,
        x.StorageKey
    })
    .IsUnique();

    builder.HasIndex(x => new
    {
        x.ApplicationId,
        x.IsDeleted
    });

    builder.HasOne(x => x.Application)
        .WithMany()
        .HasForeignKey(x => x.ApplicationId)
        .OnDelete(DeleteBehavior.Cascade);
}


}
