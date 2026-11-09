namespace Mebabl.Platform.Application.Common.Interfaces;

public interface IRealtimeConnectionManager
{
    bool RegisterConnection(
        Guid applicationId,
        Guid userId,
        string connectionId);

    bool UnregisterConnection(
        Guid applicationId,
        Guid userId,
        string connectionId);

    Task RemoveUserFromConversationAsync(
        Guid applicationId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken);
}