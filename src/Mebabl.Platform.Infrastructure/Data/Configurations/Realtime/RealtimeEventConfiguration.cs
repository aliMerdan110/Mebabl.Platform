using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Realtime;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Realtime;

public sealed class RealtimeEventConfiguration 
    : IEntityTypeConfiguration<RealtimeEvent>
{
    public void Configure(
        EntityTypeBuilder<RealtimeEvent> builder)
    {
        builder.ToTable("RealtimeEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();


        builder.Property(x => x.Payload)
            .HasColumnType("jsonb");
    }
}