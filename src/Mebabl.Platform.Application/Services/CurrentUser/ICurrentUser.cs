namespace Mebabl.Platform.Application.Services.CurrentUser;

public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? AccountId { get; }

    Guid? ApplicationId { get; }

    Guid? TenantId { get; }

    bool IsAuthenticated { get; }
}