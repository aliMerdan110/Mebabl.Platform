using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Social;

public sealed class SocialReactionConfiguration
    : IEntityTypeConfiguration<SocialReaction>
{
    public void Configure(EntityTypeBuilder<SocialReaction> builder)
    {
        builder.ToTable("SocialReactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.PostId,
            x.UserId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.PostId
        });
    }
}