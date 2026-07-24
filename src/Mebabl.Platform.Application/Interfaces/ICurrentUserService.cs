namespace Mebabl.Platform.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    Guid? AccountId { get; }

    Guid? TenantId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }
}