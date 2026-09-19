using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Domain.Entities;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Storage;
using Mebabl.Platform.Domain.Entities.Realtime;
using Mebabl.Platform.Domain.Entities.Notifications;
using Mebabl.Platform.Domain.Modules.Chat.Entities;
using Mebabl.Platform.Domain.Entities.Applications;
using Mebabl.Platform.Domain.Live;
using Mebabl.Platform.Domain.Entities.Content;
using Mebabl.Platform.Domain.Entities.Social;
using Mebabl.Platform.Domain.Entities.Commerce;

namespace Mebabl.Platform.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Developer> Developers { get; }

    DbSet<DeveloperPasswordResetToken> DeveloperPasswordResetTokens { get; }

    DbSet<DeveloperRefreshToken> DeveloperRefreshTokens { get; }

    // Applications
    DbSet<ApplicationPlatform> ApplicationPlatforms { get; }

    DbSet<PlatformApplication> Applications { get; }

    DbSet<ApplicationCredential> ApplicationCredentials { get; }

    DbSet<ApplicationAuthProvider> ApplicationAuthProviders { get; }

    DbSet<ApplicationAuthenticationSettings>
        ApplicationAuthenticationSettings { get; }

    DbSet<ApplicationMobileAppLink>
        ApplicationMobileAppLinks { get; }

    // Identity
    DbSet<Account> Accounts { get; }

    DbSet<ApplicationUser> ApplicationUsers { get; }

    DbSet<ApplicationUserEmailVerificationToken>
        ApplicationUserEmailVerificationTokens { get; }

    DbSet<ApplicationUserPasswordResetToken>
        ApplicationUserPasswordResetTokens { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<Role> Roles { get; }

    DbSet<RolePermission> RolePermissions { get; }

    DbSet<ApplicationUserRole> ApplicationUserRoles { get; }

    DbSet<SecurityRule> SecurityRules { get; }

    // Live
    DbSet<LiveStream> LiveStreams { get; }

    DbSet<StreamCredential> StreamCredentials { get; }

    DbSet<LiveStreamSession> LiveStreamSessions { get; }

    // Database
    DbSet<Collection> Collections { get; }

    DbSet<Document> Documents { get; }

    // Storage
    DbSet<Bucket> Buckets { get; }

    DbSet<StoredFile> StoredFiles { get; }

    


// content

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
    DbSet<Mebabl.Platform.Domain.Entities.Notifications.Notification>
        Notifications { get; }

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

// 


    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}