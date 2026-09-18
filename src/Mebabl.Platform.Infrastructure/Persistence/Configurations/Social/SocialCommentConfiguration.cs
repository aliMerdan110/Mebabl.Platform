using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Social;

public sealed class SocialCommentConfiguration
    : IEntityTypeConfiguration<SocialComment>
{
    public void Configure(EntityTypeBuilder<SocialComment> builder)
    {
        builder.ToTable("SocialComments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .HasMaxLength(5000)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.PostId,
            x.CreatedAt
        });

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.OwnerId
        });

        builder.HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}