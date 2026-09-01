using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Chat;

public sealed class MessageAttachmentConfiguration
    : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(
        EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.ToTable("MessageAttachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Caption)
            .HasMaxLength(500);

        builder.HasIndex(x => x.MessageId);

        builder.HasIndex(x => x.StoredFileId)
            .IsUnique();

        builder.HasOne(x => x.Message)
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StoredFile)
            .WithMany()
            .HasForeignKey(x => x.StoredFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}