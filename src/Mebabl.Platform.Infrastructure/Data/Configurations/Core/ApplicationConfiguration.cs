using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public class ApplicationConfiguration
    : IEntityTypeConfiguration<PlatformApplication>
{
    public void Configure(EntityTypeBuilder<PlatformApplication> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        

        builder.HasOne(x => x.Developer)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);


       builder.HasMany(x => x.Credentials)
    .WithOne(x => x.Application)
    .HasForeignKey(x => x.ApplicationId)
    .OnDelete(DeleteBehavior.Cascade);

    }
}