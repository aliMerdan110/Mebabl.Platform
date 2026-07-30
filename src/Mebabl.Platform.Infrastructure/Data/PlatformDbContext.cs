using Microsoft.EntityFrameworkCore.ChangeTracking;
using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities;
using Mebabl.Platform.Domain.Modules.Social.Entities;
using Mebabl.Platform.Domain.Modules.Chat.Entities;
using Mebabl.Platform.Domain.Modules.Store.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    // Core

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<PlatformApplication> Applications => Set<PlatformApplication>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();


    // Social

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<Media> Medias => Set<Media>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Like> Likes => Set<Like>();

    public DbSet<Follow> Follows => Set<Follow>();

    public DbSet<Share> Shares => Set<Share>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Mention> Mentions => Set<Mention>();


    // Chat

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();

    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();

    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();

    public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();


    // Store

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductMedia> ProductMedias => Set<ProductMedia>();

    public DbSet<Inventory> Inventories => Set<Inventory>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Shipment> Shipments => Set<Shipment>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Wishlist> Wishlists => Set<Wishlist>();

    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    public DbSet<Coupon> Coupons => Set<Coupon>();

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