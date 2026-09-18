using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Content;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations.Content;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .HasMaxLength(10000);

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.CreatedAt
        });

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.OwnerId
        });

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}