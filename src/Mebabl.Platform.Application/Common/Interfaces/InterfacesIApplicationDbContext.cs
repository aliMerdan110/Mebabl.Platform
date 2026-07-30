using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Domain.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }

    DbSet<PlatformApplication> Applications { get; }

    DbSet<Account> Accounts { get; }

    DbSet<ApplicationUser> ApplicationUsers { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}