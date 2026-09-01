using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Mebabl.Platform.Infrastructure.Realtime;

[Authorize]
public sealed class RealtimeHub : Hub
{
    public async Task Subscribe(Guid channelId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            channelId.ToString());
    }

    public async Task Unsubscribe(Guid channelId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            channelId.ToString());
    }


    // جاري الكتابه
 public async Task StartTyping(Guid conversationId)
{
    await Groups.AddToGroupAsync(
        Context.ConnectionId,
        conversationId.ToString());

    await Clients.OthersInGroup(
        conversationId.ToString())
        .SendAsync(
            "userTyping",
            new
            {
                conversationId,
                userId = GetUserId()
            });
}

public async Task StopTyping(Guid conversationId)
{
    await Groups.AddToGroupAsync(
        Context.ConnectionId,
        conversationId.ToString());

    await Clients.OthersInGroup(
        conversationId.ToString())
        .SendAsync(
            "userStoppedTyping",
            new
            {
                conversationId,
                userId = GetUserId()
            });
}


  private Guid GetUserId()
    {
        var value = Context.User?
            .FindFirst("userId")?
            .Value;

        return Guid.Parse(value!);
    }


public async Task JoinConversation(Guid conversationId)
{
    await Groups.AddToGroupAsync(
        Context.ConnectionId,
        conversationId.ToString());
}

public async Task LeaveConversation(Guid conversationId)
{
    await Groups.RemoveFromGroupAsync(
        Context.ConnectionId,
        conversationId.ToString());
}


// online/offline status
private Guid GetApplicationId()
{
    var value = Context.User?
        .FindFirst("applicationId")?
        .Value;

    return Guid.Parse(value!);
}

public override async Task OnConnectedAsync()
{
    var userId = GetUserId();
    var applicationId = GetApplicationId();

    var applicationGroup = $"application:{applicationId}";

    await Groups.AddToGroupAsync(
        Context.ConnectionId,
        applicationGroup);

    await Clients.OthersInGroup(applicationGroup)
        .SendAsync(
            "userOnline",
            new
            {
                userId
            });

    await base.OnConnectedAsync();
}

public override async Task OnDisconnectedAsync(
    Exception? exception)
{
    var userId = GetUserId();
    var applicationId = GetApplicationId();

    var applicationGroup = $"application:{applicationId}";

    await Clients.OthersInGroup(applicationGroup)
        .SendAsync(
            "userOffline",
            new
            {
                userId
            });

    await base.OnDisconnectedAsync(exception);
}

    
}