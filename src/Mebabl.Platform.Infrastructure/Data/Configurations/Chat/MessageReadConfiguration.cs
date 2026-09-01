using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Chat;

public sealed class MessageReadConfiguration
    : IEntityTypeConfiguration<MessageRead>
{
    public void Configure(
        EntityTypeBuilder<MessageRead> builder)
    {
        builder.ToTable("MessageReads");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.MessageId,
            x.UserId
        })
        .IsUnique();

        builder.HasOne(x => x.Message)
            .WithMany(x => x.Reads)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}