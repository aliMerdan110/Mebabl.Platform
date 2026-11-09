using Microsoft.AspNetCore.SignalR;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Realtime;

public sealed class RealtimeConnectionManager
    : IRealtimeConnectionManager
{
    private readonly IHubContext<RealtimeHub> _hubContext;
    private readonly object _sync = new();

    private readonly Dictionary<
        (Guid ApplicationId, Guid UserId),
        HashSet<string>> _connections = new();

    public RealtimeConnectionManager(
        IHubContext<RealtimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public bool RegisterConnection(
        Guid applicationId,
        Guid userId,
        string connectionId)
    {
        ValidateIdentifiers(applicationId, userId, connectionId);

        lock (_sync)
        {
            var key = (applicationId, userId);

            if (!_connections.TryGetValue(key, out var connections))
            {
                connections = new HashSet<string>(
                    StringComparer.Ordinal);

                _connections.Add(key, connections);
            }

            var added = connections.Add(connectionId);

            return added && connections.Count == 1;
        }
    }

    public bool UnregisterConnection(
        Guid applicationId,
        Guid userId,
        string connectionId)
    {
        ValidateIdentifiers(applicationId, userId, connectionId);

        lock (_sync)
        {
            var key = (applicationId, userId);

            if (!_connections.TryGetValue(key, out var connections))
            {
                return false;
            }

            connections.Remove(connectionId);

            if (connections.Count > 0)
            {
                return false;
            }

            _connections.Remove(key);
            return true;
        }
    }

    public async Task RemoveUserFromConversationAsync(
        Guid applicationId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        if (applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            conversationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Valid application, user, and conversation IDs are required.");
        }

        string[] connectionIds;

        lock (_sync)
        {
            if (!_connections.TryGetValue(
                    (applicationId, userId),
                    out var connections))
            {
                return;
            }

            connectionIds = connections.ToArray();
        }

        var groupName = conversationId.ToString();

        foreach (var connectionId in connectionIds)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(
                connectionId,
                groupName,
                cancellationToken);
        }
    }

    private static void ValidateIdentifiers(
        Guid applicationId,
        Guid userId,
        string connectionId)
    {
        if (applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            string.IsNullOrWhiteSpace(connectionId))
        {
            throw new ArgumentException(
                "Valid application, user, and connection identifiers are required.");
        }
    }
}