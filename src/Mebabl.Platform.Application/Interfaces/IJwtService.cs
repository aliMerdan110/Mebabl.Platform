namespace Mebabl.Platform.Application.Interfaces;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(Guid userId);

    Task<string> GenerateRefreshTokenAsync();
}