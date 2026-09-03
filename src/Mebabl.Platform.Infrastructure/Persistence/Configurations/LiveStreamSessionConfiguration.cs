// Infrastructure/Persistence/Configurations/LiveStreamSessionConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Live;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class LiveStreamSessionConfiguration
    : IEntityTypeConfiguration<LiveStreamSession>
{
    public void Configure(EntityTypeBuilder<LiveStreamSession> builder)
    {
        builder.ToTable("LiveStreamSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublisherUserId)
            .IsRequired();

        builder.Property(x => x.PublishTokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.PublishTokenHash)
            .IsUnique();

        builder.Property(x => x.PublishTokenExpiresAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.StartedAt);

        builder.Property(x => x.EndedAt);

        builder.HasIndex(x => new
        {
            x.LiveStreamId,
            x.Status
        });

        builder.HasIndex(x => new
        {
            x.LiveStreamId,
            x.PublisherUserId
        });

        // PostgreSQL:
        // يمنع وجود أكثر من Session غير منتهية لنفس Stream.
        builder.HasIndex(x => x.LiveStreamId)
            .IsUnique()
            .HasFilter("\"Status\" <> 2");

        builder.HasOne(x => x.LiveStream)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}