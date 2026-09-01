using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Notifications;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Notifications;

public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(
        EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Data)
            .HasColumnType("jsonb");

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.UserId,
            x.IsRead
        });

        builder.HasIndex(x => x.CreatedAt);
    }
}