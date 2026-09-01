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





namespace Mebabl.Platform.Infrastructure.Data;

public class PlatformDbContext : DbContext, IApplicationDbContext
{
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

    
    
    // أضف هذا السطر هنا ليكون متاحاً لـ SDK Users
    public DbSet<ApplicationUserPasswordResetToken> ApplicationUserPasswordResetTokens => Set<ApplicationUserPasswordResetToken>();


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
    // Core
//  خاصية اضافه معلومات التطبيق عند الانشاء 
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




    // 


   public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
{
    UpdateAuditableEntities();

    return await base.SaveChangesAsync(cancellationToken);
}

private void UpdateAuditableEntities()
{
    var entries = ChangeTracker.Entries<AuditableEntity>();

    foreach (var entry in entries)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.CreatedAt = _clock.UtcNow;
                entry.Entity.CreatedBy = _currentUser.UserId;
                break;

            case EntityState.Modified:
                entry.Entity.UpdatedAt = _clock.UtcNow;
                entry.Entity.UpdatedBy = _currentUser.UserId;
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