namespace Mebabl.Platform.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }

    Guid AccountId { get; }

    Guid ApplicationId { get; }


    bool IsAuthenticated { get; }
}