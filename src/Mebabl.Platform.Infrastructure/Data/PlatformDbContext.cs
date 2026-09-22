using Microsoft.EntityFrameworkCore.ChangeTracking;
using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Application.Services.Clock;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Storage;
using Mebabl.Platform.Domain.Entities.Realtime;
using Mebabl.Platform.Domain.Entities.Notifications;
using Mebabl.Platform.Domain.Modules.Chat.Entities;
using Mebabl.Platform.Domain.Entities.Applications;
using Mebabl.Platform.Domain.Live;
using Mebabl.Platform.Domain.Entities.Social;
using Mebabl.Platform.Domain.Entities.Commerce;
using Mebabl.Platform.Domain.Entities.Projects;


namespace Mebabl.Platform.Infrastructure.Data;

public class PlatformDbContext : DbContext, IApplicationDbContext
{


    public DbSet<ProjectSdkConfiguration> ProjectSdkConfigurations
    => Set<ProjectSdkConfiguration>();

    public DbSet<PlatformProject> Projects
    => Set<PlatformProject>();


    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;




    public PlatformDbContext(
        DbContextOptions<PlatformDbContext> options,
        IClock clock,
        ICurrentUser currentUser)
        : base(options)
    {
        _clock = clock;
        _currentUser = currentUser;
    }

    public DbSet<DeveloperPasswordResetToken> DeveloperPasswordResetTokens { get; set; }

    public DbSet<ApplicationUserEmailVerificationToken>
        ApplicationUserEmailVerificationTokens
        => Set<ApplicationUserEmailVerificationToken>();
    
    public DbSet<ApplicationUserPasswordResetToken> ApplicationUserPasswordResetTokens => Set<ApplicationUserPasswordResetToken>();

    public DbSet<LiveStream> LiveStreams => Set<LiveStream>();

    public DbSet<StreamCredential> StreamCredentials => Set<StreamCredential>();

    public DbSet<LiveStreamSession> LiveStreamSessions => Set<LiveStreamSession>();

    public DbSet<Developer> Developers => Set<Developer>();

    public DbSet<Collection> Collections => Set<Collection>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Bucket> Buckets => Set<Bucket>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<DeveloperRefreshToken> DeveloperRefreshTokens
        => Set<DeveloperRefreshToken>();

    public DbSet<Mebabl.Platform.Domain.Entities.Notifications.Notification>
        Notifications
        => Set<Mebabl.Platform.Domain.Entities.Notifications.Notification>();

    public DbSet<ApplicationPlatform> ApplicationPlatforms
        => Set<ApplicationPlatform>();

    public DbSet<ApplicationCredential> ApplicationCredentials
        => Set<ApplicationCredential>();

    public DbSet<ApplicationAuthProvider> ApplicationAuthProviders
        => Set<ApplicationAuthProvider>();

    public DbSet<PlatformApplication> Applications => Set<PlatformApplication>();

    public DbSet<ApplicationAuthenticationSettings>
        ApplicationAuthenticationSettings
        => Set<ApplicationAuthenticationSettings>();

    public DbSet<ApplicationMobileAppLink> ApplicationMobileAppLinks
        => Set<ApplicationMobileAppLink>();
    
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();

    public DbSet<SecurityRule> SecurityRules => Set<SecurityRule>();

    public DbSet<Channel> Channels { get; set; } = default!;

    public DbSet<RealtimeEvent> RealtimeEvents { get; set; } = default!;

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ConversationParticipant> ConversationParticipants
        => Set<ConversationParticipant>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<MessageRead> MessageReads
        => Set<MessageRead>();

    public DbSet<MessageReaction> MessageReactions
        => Set<MessageReaction>();

    public DbSet<MessageAttachment> MessageAttachments
        => Set<MessageAttachment>();

    public DbSet<Mebabl.Platform.Domain.Entities.Content.Post> Posts
        => Set<Mebabl.Platform.Domain.Entities.Content.Post>();

    public DbSet<Mebabl.Platform.Domain.Entities.Content.PostAttachment> PostAttachments
        => Set<Mebabl.Platform.Domain.Entities.Content.PostAttachment>();

    public DbSet<SocialReaction> SocialReactions { get; set; }

    public DbSet<SocialComment> SocialComments { get; set; }

    public DbSet<SocialShare> SocialShares { get; set; }

    public DbSet<SocialRepost> SocialReposts { get; set; }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        
        // التحقق الآمن مما إذا كان المستخدم مسجلاً لتجنب الاستثناءات في نقاط النهاية العامة (مثل Register)
        Guid? userId = _currentUser.IsAuthenticated ? _currentUser.UserId : null;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = _clock.UtcNow;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = _clock.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PlatformDbContext).Assembly);
    }
}