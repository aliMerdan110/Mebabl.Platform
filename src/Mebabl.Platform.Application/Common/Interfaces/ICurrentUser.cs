namespace Mebabl.Platform.Application.Common.Interfaces;

/// <summary>
/// Provides the authenticated end-user identity and its resource scope.
/// The project and application identifiers must come from validated token claims,
/// never from request payloads or client-controlled headers.
/// </summary>
public interface ICurrentUser
{
    Guid UserId { get; }

    Guid AccountId { get; }

    Guid ApplicationId { get; }

    Guid ProjectId { get; }

    bool IsAuthenticated { get; }
}
