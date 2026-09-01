using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Chat;

public sealed class MessageReactionConfiguration
    : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(
        EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("MessageReactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reaction)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new
        {
            x.MessageId,
            x.UserId
        })
        .IsUnique();

        builder.HasOne(x => x.Message)
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}