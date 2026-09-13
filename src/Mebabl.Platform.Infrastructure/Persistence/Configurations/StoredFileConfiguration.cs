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

        builder.Property(x => x.Path)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .HasMaxLength(1200)
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

        builder.HasOne(x => x.Application)
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}