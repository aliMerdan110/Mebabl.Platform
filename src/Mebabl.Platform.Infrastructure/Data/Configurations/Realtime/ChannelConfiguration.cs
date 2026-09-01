using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Realtime;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Realtime;

public sealed class ChannelConfiguration 
    : IEntityTypeConfiguration<Channel>
{
    public void Configure(
        EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("Channels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();


        builder.HasMany(x => x.Events)
            .WithOne(x => x.Channel)
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}