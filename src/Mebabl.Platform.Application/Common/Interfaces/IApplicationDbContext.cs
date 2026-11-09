using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Mebabl.Platform.Domain.Entities.Content;
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

namespace Mebabl.Platform.Application.Common.Interfaces;

/// <summary>
/// Application-facing persistence contract.
/// The project-owned Database module must always apply ProjectId scope in its
/// queries; exposing a DbSet does not grant cross-project access.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Provides EF Core execution-strategy and transaction facilities.
    /// Use the provider execution strategy when retrying explicit transactions.
    /// </summary>
    DatabaseFacade Database { get; }

    // Projects
    DbSet<PlatformProject> Projects { get; }
    DbSet<ProjectPlatform> ProjectPlatforms { get; }
    DbSet<ProjectPlatformFingerprint> ProjectPlatformFingerprints { get; }
    DbSet<ProjectSdkConfiguration> ProjectSdkConfigurations { get; }

    // Developers
    DbSet<Developer> Developers { get; }
    DbSet<DeveloperPasswordResetToken> DeveloperPasswordResetTokens { get; }
    DbSet<DeveloperRefreshToken> DeveloperRefreshTokens { get; }

    Task<Guid?> RotateDeveloperRefreshTokenAsync(
        string currentToken,
        string currentTokenHash,
        string newTokenHash,
        DateTime now,
        DateTime newExpiresAt,
        CancellationToken cancellationToken);

    // Applications
    DbSet<PlatformApplication> Applications { get; }
    DbSet<ApplicationCredential> ApplicationCredentials { get; }
    DbSet<ApplicationAuthProvider> ApplicationAuthProviders { get; }
    DbSet<ApplicationAuthenticationSettings> ApplicationAuthenticationSettings { get; }
    DbSet<ApplicationMobileAppLink> ApplicationMobileAppLinks { get; }

    // Identity and authorization
    DbSet<Account> Accounts { get; }
    DbSet<ApplicationUser> ApplicationUsers { get; }
    DbSet<ApplicationUserEmailVerificationToken> ApplicationUserEmailVerificationTokens { get; }
    DbSet<ApplicationUserPasswordResetToken> ApplicationUserPasswordResetTokens { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<ApplicationUserRole> ApplicationUserRoles { get; }

    // Database
    DbSet<Collection> Collections { get; }
    DbSet<Document> Documents { get; }
    DbSet<SecurityRule> SecurityRules { get; }

    // Live
    DbSet<LiveStream> LiveStreams { get; }
    DbSet<StreamCredential> StreamCredentials { get; }
    DbSet<LiveStreamSession> LiveStreamSessions { get; }

    // Storage
    DbSet<Bucket> Buckets { get; }
    DbSet<StoredFile> StoredFiles { get; }

    // Content
    DbSet<Post> Posts { get; }
    DbSet<PostAttachment> PostAttachments { get; }

    // Social
    DbSet<SocialReaction> SocialReactions { get; }
    DbSet<SocialComment> SocialComments { get; }
    DbSet<SocialShare> SocialShares { get; }
    DbSet<SocialRepost> SocialReposts { get; }

    // Realtime
    DbSet<Channel> Channels { get; }
    DbSet<RealtimeEvent> RealtimeEvents { get; }

    // Notifications
    DbSet<Notification> Notifications { get; }

    // Chat
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationParticipant> ConversationParticipants { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageRead> MessageReads { get; }
    DbSet<MessageReaction> MessageReactions { get; }
    DbSet<MessageAttachment> MessageAttachments { get; }

    // Commerce
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}
