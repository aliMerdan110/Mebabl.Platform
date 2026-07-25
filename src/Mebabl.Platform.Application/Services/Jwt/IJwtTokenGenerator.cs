namespace Mebabl.Platform.Application.Services.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId);

    string GenerateRefreshToken();
}