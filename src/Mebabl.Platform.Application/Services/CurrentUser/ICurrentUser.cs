namespace Mebabl.Platform.Application.Services.CurrentUser;

public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? AccountId { get; }

    Guid? TenantId { get; }

    Guid? ApplicationId { get; }

    bool IsAuthenticated { get; }
}