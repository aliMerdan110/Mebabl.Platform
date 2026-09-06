using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Infrastructure.Services.Clock;

namespace Mebabl.Platform.Infrastructure.Data;

public sealed class PlatformDbContextFactory
    : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                "appsettings.json",
                optional: true)
            .AddJsonFile(
                "appsettings.Development.json",
                optional: true)
            .AddUserSecrets<PlatformDbContextFactory>(
                optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection was not found.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<PlatformDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new PlatformDbContext(
            optionsBuilder.Options,
            new Clock(),
            new DesignTimeCurrentUser());
    }
}

internal sealed class DesignTimeCurrentUser : ICurrentUser
{
    public Guid UserId => Guid.Empty;
    public Guid AccountId => Guid.Empty;
    public Guid ApplicationId => Guid.Empty;
    public bool IsAuthenticated => false;
}