using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Social;

public sealed class SocialShareConfiguration
    : IEntityTypeConfiguration<SocialShare>
{
    public void Configure(EntityTypeBuilder<SocialShare> builder)
    {
        builder.ToTable("SocialShares");

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