namespace Mebabl.Platform.Application.Services.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(
    Guid accountId,
    Guid userId,
    Guid applicationId,
    Guid tenantId);

    string GenerateRefreshToken();
}