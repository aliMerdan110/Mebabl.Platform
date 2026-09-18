using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Social;

public sealed class SocialRepostConfiguration
    : IEntityTypeConfiguration<SocialRepost>
{
    public void Configure(EntityTypeBuilder<SocialRepost> builder)
    {
        builder.ToTable("SocialReposts");

        builder.HasKey(x => x.Id);

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