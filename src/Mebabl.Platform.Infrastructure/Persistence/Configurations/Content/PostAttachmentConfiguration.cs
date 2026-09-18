using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Content;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Content;

public sealed class PostAttachmentConfiguration
    : IEntityTypeConfiguration<PostAttachment>
{
    public void Configure(EntityTypeBuilder<PostAttachment> builder)
    {
        builder.ToTable("PostAttachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.PostId,
            x.Order
        });

        builder.HasIndex(x => x.StorageFileId);

        builder.HasOne(x => x.StorageFile)
            .WithMany()
            .HasForeignKey(x => x.StorageFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}