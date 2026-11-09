using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Commerce;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Notifications;
using Mebabl.Platform.Domain.Entities.Projects;
using Mebabl.Platform.Domain.Entities.Realtime;
using Mebabl.Platform.Domain.Entities.Social;
using Mebabl.Platform.Domain.Entities.Storage;
using Mebabl.Platform.Domain.Live;
using Mebabl.Platform.Domain.Modules.Chat.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mebabl.Platform.Infrastructure.Data;

/// <summary>
/// Main EF Core persistence context.
/// Database collections and documents are project-owned. Every read/query
/// path must still apply an explicit ProjectId predicate; DbSet exposure is
/// not an authorization boundary.
/// </summary>
public sealed class PlatformDbContext : DbContext, IApplicationDbContext
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public PlatformDbContext(
        DbContextOptions<PlatformDbContext> options,
        IClock clock,
        ICurrentUser currentUser)
        : base(options)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    // Projects
    public DbSet<PlatformProject> Projects => Set<PlatformProject>();
    public DbSet<ProjectPlatform> ProjectPlatforms => Set<ProjectPlatform>();
    public DbSet<ProjectPlatformFingerprint> ProjectPlatformFingerprints =>
        Set<ProjectPlatformFingerprint>();
    public DbSet<ProjectSdkConfiguration> ProjectSdkConfigurations =>
        Set<ProjectSdkConfiguration>();

    // Developers
    public DbSet<Developer> Developers => Set<Developer>();
    public DbSet<DeveloperPasswordResetToken> DeveloperPasswordResetTokens =>
        Set<DeveloperPasswordResetToken>();
    public DbSet<DeveloperRefreshToken> DeveloperRefreshTokens =>
        Set<DeveloperRefreshToken>();

    // Applications
    public DbSet<PlatformApplication> Applications => Set<PlatformApplication>();
    public DbSet<ApplicationCredential> ApplicationCredentials =>
        Set<ApplicationCredential>();
    public DbSet<ApplicationAuthProvider> ApplicationAuthProviders =>
        Set<ApplicationAuthProvider>();
    public DbSet<ApplicationAuthenticationSettings> ApplicationAuthenticationSettings =>
        Set<ApplicationAuthenticationSettings>();
    public DbSet<ApplicationMobileAppLink> ApplicationMobileAppLinks =>
        Set<ApplicationMobileAppLink>();

    // Identity and authorization
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<ApplicationUserEmailVerificationToken> ApplicationUserEmailVerificationTokens =>
        Set<ApplicationUserEmailVerificationToken>();
    public DbSet<ApplicationUserPasswordResetToken> ApplicationUserPasswordResetTokens =>
        Set<ApplicationUserPasswordResetToken>();
    public DbSet<ApplicationUserRole> ApplicationUserRoles =>
        Set<ApplicationUserRole>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Project-owned Database
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<SecurityRule> SecurityRules => Set<SecurityRule>();

    // Storage
    public DbSet<Bucket> Buckets => Set<Bucket>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    // Realtime
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<RealtimeEvent> RealtimeEvents => Set<RealtimeEvent>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();

    // Chat
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants =>
        Set<ConversationParticipant>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageRead> MessageReads => Set<MessageRead>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();

    // Content
    public DbSet<Mebabl.Platform.Domain.Entities.Content.Post> Posts =>
        Set<Mebabl.Platform.Domain.Entities.Content.Post>();
    public DbSet<Mebabl.Platform.Domain.Entities.Content.PostAttachment> PostAttachments =>
        Set<Mebabl.Platform.Domain.Entities.Content.PostAttachment>();

    // Social
    public DbSet<SocialReaction> SocialReactions => Set<SocialReaction>();
    public DbSet<SocialComment> SocialComments => Set<SocialComment>();
    public DbSet<SocialShare> SocialShares => Set<SocialShare>();
    public DbSet<SocialRepost> SocialReposts => Set<SocialRepost>();

    // Live
    public DbSet<LiveStream> LiveStreams => Set<LiveStream>();
    public DbSet<StreamCredential> StreamCredentials => Set<StreamCredential>();
    public DbSet<LiveStreamSession> LiveStreamSessions => Set<LiveStreamSession>();

    // Commerce
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Atomically revokes the current developer refresh token and creates
    /// its replacement. The old hash is retained to support replay detection.
    /// </summary>
    public async Task<Guid?> RotateDeveloperRefreshTokenAsync(
        string currentToken,
        string currentTokenHash,
        string newTokenHash,
        DateTime now,
        DateTime newExpiresAt,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentToken);
        ValidateSha256Hash(currentTokenHash, nameof(currentTokenHash));
        ValidateSha256Hash(newTokenHash, nameof(newTokenHash));

        if (newExpiresAt <= now)
        {
            throw new ArgumentOutOfRangeException(
                nameof(newExpiresAt),
                "The replacement refresh token must expire after it is issued.");
        }

        await using var transaction =
            await Database.BeginTransactionAsync(cancellationToken);

        var current = await DeveloperRefreshTokens
            .AsNoTracking()
            .Where(x => x.TokenHash == currentTokenHash || x.Token == currentToken)
            .Select(x => new { x.Id, x.DeveloperId })
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var affectedRows = await DeveloperRefreshTokens
            .Where(x =>
                x.Id == current.Id &&
                (x.TokenHash == currentTokenHash || x.Token == currentToken) &&
                x.RevokedAt == null &&
                x.ExpiresAt > now &&
                x.Developer.IsActive &&
                !x.Developer.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.RevokedAt, (DateTime?)now)
                    .SetProperty(x => x.Token, (string?)null)
                    .SetProperty(x => x.TokenHash, currentTokenHash),
                cancellationToken);

        if (affectedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        DeveloperRefreshTokens.Add(new DeveloperRefreshToken
        {
            DeveloperId = current.DeveloperId,
            Token = null,
            TokenHash = newTokenHash,
            ExpiresAt = newExpiresAt
        });

        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return current.DeveloperId;
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PrepareChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PrepareChanges()
    {
        EnforceAuthenticatedProjectWriteScope();
        UpdateAuditableEntities();
    }

    /// <summary>
    /// Defense in depth for user-authenticated document writes. Query handlers
    /// and services must independently scope reads and writes by ProjectId.
    /// Non-user system/developer flows retain their dedicated authorization path.
    /// </summary>
    private void EnforceAuthenticatedProjectWriteScope()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return;
        }

        var authenticatedProjectId = _currentUser.ProjectId;
        if (authenticatedProjectId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated user does not have a valid project scope.");
        }

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            var entityProjectId = entry.Entity switch
            {
                Collection collection => collection.ProjectId,
                Document document => document.ProjectId,
                SecurityRule securityRule => securityRule.ProjectId,
                _ => (Guid?)null
            };

            if (!entityProjectId.HasValue)
            {
                continue;
            }

            // Check both the current and original values. Otherwise, a tracked
            // entity loaded from another project could be reassigned to the
            // authenticated project before SaveChanges and pass this guard.
            // ChangeTracker.Entries() returns non-generic EntityEntry, so read
            // the original value through the non-generic Property(string) API.
            var originalProjectId = (Guid)entry
                .Property(nameof(Collection.ProjectId))
                .OriginalValue!;

            if (entityProjectId.Value != authenticatedProjectId ||
                (entry.State != EntityState.Added && originalProjectId != authenticatedProjectId))
            {
                throw new UnauthorizedAccessException(
                    "The authenticated user cannot modify Database resources outside the current project.");
            }
        }
    }

    private void UpdateAuditableEntities()
    {
        var now = _clock.UtcNow;
        var userId = _currentUser.IsAuthenticated && _currentUser.UserId != Guid.Empty
            ? _currentUser.UserId
            : (Guid?)null;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    // Creation metadata is immutable after insertion.
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }

    private static void ValidateSha256Hash(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(
                "Refresh token hashes must be 64 hexadecimal characters.",
                parameterName);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);
}
