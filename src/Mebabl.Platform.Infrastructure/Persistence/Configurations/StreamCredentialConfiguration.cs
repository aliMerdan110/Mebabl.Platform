// Infrastructure/Persistence/Configurations/StreamCredentialConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Live;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class StreamCredentialConfiguration
    : IEntityTypeConfiguration<StreamCredential>
{
    public void Configure(EntityTypeBuilder<StreamCredential> builder)
    {
        builder.ToTable("StreamCredentials");

        builder.HasKey(x => x.Id);

        // لا يتم تخزين StreamKey الخام.
        builder.Property(x => x.KeyHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.KeyHash)
            .IsUnique();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.LiveStream)
            .WithMany(x => x.Credentials)
            .HasForeignKey(x => x.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

