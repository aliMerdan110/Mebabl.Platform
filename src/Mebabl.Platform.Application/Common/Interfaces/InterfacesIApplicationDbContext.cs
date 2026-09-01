using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Domain.Entities;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Storage;
using Mebabl.Platform.Domain.Entities.Realtime;
using Mebabl.Platform.Domain.Entities.Notifications;
using Mebabl.Platform.Domain.Modules.Chat.Entities;
using Mebabl.Platform.Domain.Entities.Applications;



namespace Mebabl.Platform.Application.Common.Interfaces;

public interface IApplicationDbContext
{
      
   
    DbSet<Developer> Developers { get; }

    DbSet<DeveloperPasswordResetToken> DeveloperPasswordResetTokens { get; }

    DbSet<DeveloperRefreshToken> DeveloperRefreshTokens { get; }

    
    
    DbSet<ApplicationPlatform> ApplicationPlatforms { get; }


//    اضافه تطبيق جديد
    DbSet<PlatformApplication> Applications { get; }

    // تاكيد الحساب عن طريق البريد الالكتروني
    DbSet<ApplicationUserEmailVerificationToken>ApplicationUserEmailVerificationTokens { get; }

    DbSet<ApplicationCredential> ApplicationCredentials { get; }

    DbSet<ApplicationUserPasswordResetToken> ApplicationUserPasswordResetTokens { get; }

    DbSet<ApplicationAuthProvider> ApplicationAuthProviders { get; }

    DbSet<ApplicationAuthenticationSettings>
    ApplicationAuthenticationSettings { get; }


    DbSet<Account> Accounts { get; }

    DbSet<ApplicationUser> ApplicationUsers { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<Role> Roles { get; }

    DbSet<RolePermission> RolePermissions { get; }

    DbSet<ApplicationUserRole> ApplicationUserRoles { get; }

     DbSet<Collection> Collections { get; }

    DbSet<Document> Documents { get; }

    DbSet<Bucket> Buckets { get; }

    DbSet<StoredFile> StoredFiles { get; }

    DbSet<Channel> Channels { get; }

DbSet<RealtimeEvent> RealtimeEvents { get; }

DbSet<Mebabl.Platform.Domain.Entities.Notifications.Notification> Notifications { get; }



// Chat
DbSet<Conversation> Conversations { get; }
DbSet<ConversationParticipant> ConversationParticipants { get; }
DbSet<Message> Messages { get; }

DbSet<MessageRead> MessageReads { get; }

DbSet<MessageReaction> MessageReactions { get; }

DbSet<MessageAttachment> MessageAttachments { get; }


// 

    DbSet<SecurityRule> SecurityRules { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}