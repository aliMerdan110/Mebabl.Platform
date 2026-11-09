using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Realtime;

[Authorize(Policy = "ApplicationUser")]
public sealed class RealtimeHub : Hub
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRealtimeConnectionManager _connectionManager;

    public RealtimeHub(
        IApplicationDbContext dbContext,
        IRealtimeConnectionManager connectionManager)
    {
        _dbContext = dbContext;
        _connectionManager = connectionManager;
    }

    public async Task Subscribe(Guid channelId)
    {
        var identity = await ValidateCurrentUserAsync();

        if (channelId == Guid.Empty)
        {
            throw new HubException("A valid channel ID is required.");
        }

        var channelIsAvailable = await _dbContext.Channels
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == channelId &&
                    x.ApplicationId == identity.ApplicationId &&
                    x.IsActive &&
                    !x.IsDeleted,
                Context.ConnectionAborted);

        if (!channelIsAvailable)
        {
            throw new HubException(
                "The channel is unavailable in this application.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            channelId.ToString(),
            Context.ConnectionAborted);
    }

    public async Task Unsubscribe(Guid channelId)
    {
        _ = await ValidateCurrentUserAsync();

        if (channelId == Guid.Empty)
        {
            throw new HubException("A valid channel ID is required.");
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            channelId.ToString(),
            Context.ConnectionAborted);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var identity = await ValidateCurrentUserAsync();

        await EnsureActiveConversationMembershipAsync(
            identity,
            conversationId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            conversationId.ToString(),
            Context.ConnectionAborted);
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        var identity = await ValidateCurrentUserAsync();

        if (conversationId == Guid.Empty)
        {
            throw new HubException(
                "A valid conversation ID is required.");
        }

        var conversationExists = await _dbContext.Conversations
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == conversationId &&
                    x.ApplicationId == identity.ApplicationId,
                Context.ConnectionAborted);

        if (!conversationExists)
        {
            throw new HubException(
                "Conversation not found in this application.");
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            conversationId.ToString(),
            Context.ConnectionAborted);
    }

    public async Task StartTyping(Guid conversationId)
    {
        var identity = await ValidateCurrentUserAsync();

        await EnsureActiveConversationMembershipAsync(
            identity,
            conversationId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            conversationId.ToString(),
            Context.ConnectionAborted);

        await Clients.OthersInGroup(conversationId.ToString())
            .SendAsync(
                "userTyping",
                new
                {
                    conversationId,
                    userId = identity.UserId
                },
                Context.ConnectionAborted);
    }

    public async Task StopTyping(Guid conversationId)
    {
        var identity = await ValidateCurrentUserAsync();

        await EnsureActiveConversationMembershipAsync(
            identity,
            conversationId);

        // Do not add the connection to the group here.
        await Clients.OthersInGroup(conversationId.ToString())
            .SendAsync(
                "userStoppedTyping",
                new
                {
                    conversationId,
                    userId = identity.UserId
                },
                Context.ConnectionAborted);
    }

    public override async Task OnConnectedAsync()
    {
        var identity = await ValidateCurrentUserAsync();

        var applicationGroup =
            $"application:{identity.ApplicationId}";

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            applicationGroup,
            Context.ConnectionAborted);

        var isFirstConnection =
            _connectionManager.RegisterConnection(
                identity.ApplicationId,
                identity.UserId,
                Context.ConnectionId);

        if (isFirstConnection)
        {
            await Clients.OthersInGroup(applicationGroup)
                .SendAsync(
                    "userOnline",
                    new { userId = identity.UserId },
                    Context.ConnectionAborted);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        try
        {
            // A disconnect can occur after account deactivation or
            // SecurityStamp rotation. Do not require a fresh DB validation
            // to remove this connection from the local registry.
            if (TryGetPresenceIdentity(
                    Context.User,
                    out var applicationId,
                    out var userId))
            {
                var isLastConnection =
                    _connectionManager.UnregisterConnection(
                        applicationId,
                        userId,
                        Context.ConnectionId);

                if (isLastConnection)
                {
                    await Clients.OthersInGroup(
                            $"application:{applicationId}")
                        .SendAsync(
                            "userOffline",
                            new { userId },
                            CancellationToken.None);
                }
            }
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    private async Task<HubIdentity> ValidateCurrentUserAsync()
    {
        var identity = GetIdentityFromClaims();
        var now = DateTime.UtcNow;

        var userIsValid = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == identity.UserId &&
                    x.AccountId == identity.AccountId &&
                    x.ApplicationId == identity.ApplicationId &&
                    x.SecurityStamp == identity.SecurityStamp &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    !x.Account.IsDeleted &&
                    x.Application.IsActive &&
                    !x.Application.IsDeleted &&
                    (!x.LockoutEnabled ||
                     !x.LockoutEnd.HasValue ||
                     x.LockoutEnd <= now),
                Context.ConnectionAborted);

        if (!userIsValid)
        {
            throw new HubException(
                "The user session is invalid or no longer active.");
        }

        return identity;
    }

    private async Task EnsureActiveConversationMembershipAsync(
        HubIdentity identity,
        Guid conversationId)
    {
        if (conversationId == Guid.Empty)
        {
            throw new HubException(
                "A valid conversation ID is required.");
        }

        var isParticipant =
            await _dbContext.ConversationParticipants
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.ConversationId == conversationId &&
                        x.UserId == identity.UserId &&
                        x.LeftAt == null &&
                        x.Conversation.ApplicationId ==
                            identity.ApplicationId &&
                        !x.Conversation.IsDeleted,
                    Context.ConnectionAborted);

        if (!isParticipant)
        {
            throw new HubException(
                "Active conversation membership is required.");
        }
    }

    private HubIdentity GetIdentityFromClaims()
    {
        var principal = Context.User;

        if (principal?.Identity?.IsAuthenticated != true ||
            principal.FindFirst("type")?.Value != "user" ||
            !Guid.TryParse(
                principal.FindFirst("applicationId")?.Value,
                out var applicationId) ||
            applicationId == Guid.Empty ||
            !Guid.TryParse(
                principal.FindFirst("userId")?.Value,
                out var userId) ||
            userId == Guid.Empty ||
            !Guid.TryParse(
                principal.FindFirst("accountId")?.Value,
                out var accountId) ||
            accountId == Guid.Empty)
        {
            throw new HubException(
                "An authenticated application user is required.");
        }

        var securityStamp =
            principal.FindFirst("securityStamp")?.Value;

        if (string.IsNullOrWhiteSpace(securityStamp))
        {
            throw new HubException(
                "The user session is missing security information.");
        }

        return new HubIdentity(
            applicationId,
            userId,
            accountId,
            securityStamp);
    }

    private static bool TryGetPresenceIdentity(
        ClaimsPrincipal? principal,
        out Guid applicationId,
        out Guid userId)
    {
        applicationId = Guid.Empty;
        userId = Guid.Empty;

        return principal?.Identity?.IsAuthenticated == true &&
               principal.FindFirst("type")?.Value == "user" &&
               Guid.TryParse(
                   principal.FindFirst("applicationId")?.Value,
                   out applicationId) &&
               applicationId != Guid.Empty &&
               Guid.TryParse(
                   principal.FindFirst("userId")?.Value,
                   out userId) &&
               userId != Guid.Empty;
    }

    private sealed record HubIdentity(
        Guid ApplicationId,
        Guid UserId,
        Guid AccountId,
        string SecurityStamp);
}