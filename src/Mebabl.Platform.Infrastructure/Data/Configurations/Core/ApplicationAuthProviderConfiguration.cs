using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationAuthProviderConfiguration
    : IEntityTypeConfiguration<ApplicationAuthProvider>
{
    public void Configure(
        EntityTypeBuilder<ApplicationAuthProvider> builder)
    {
        builder.ToTable("ApplicationAuthProviders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ConfigurationJson)
            .HasColumnType("jsonb");

        builder.HasIndex(x => new
        {
            x.ApplicationId,
            x.Provider
        })
        .IsUnique();

        builder.HasOne(x => x.Application)
            .WithMany(x => x.AuthProviders)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}