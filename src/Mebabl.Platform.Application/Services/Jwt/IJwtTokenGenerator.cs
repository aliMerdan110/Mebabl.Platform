namespace Mebabl.Platform.Application.Services.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateDeveloperToken(Guid developerId);

    string GenerateApplicationToken(
        Guid applicationId,
        Guid credentialId);

    string GenerateAccessToken(
        Guid accountId,
        Guid userId,
        Guid applicationId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);

    string GenerateRefreshToken();
}