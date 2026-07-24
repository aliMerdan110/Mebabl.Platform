namespace Mebabl.Platform.Infrastructure.Authentication;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email);
}