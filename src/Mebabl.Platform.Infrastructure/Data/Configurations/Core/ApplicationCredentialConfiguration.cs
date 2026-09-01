using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class ApplicationCredentialConfiguration
    : IEntityTypeConfiguration<ApplicationCredential>
{
    public void Configure(
        EntityTypeBuilder<ApplicationCredential> builder)
    {
        builder.ToTable("ApplicationCredentials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApiKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ApiSecretHash)
            .IsRequired();

        builder.HasIndex(x => x.ApiKey)
            .IsUnique();

        builder.HasOne(x => x.Application)
            .WithMany(x => x.Credentials)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}