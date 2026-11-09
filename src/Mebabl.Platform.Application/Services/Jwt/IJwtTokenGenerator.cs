namespace Mebabl.Platform.Application.Services.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateDeveloperToken(Guid developerId);

    string GenerateApplicationToken(
        Guid applicationId,
        Guid projectId,
        Guid credentialId);

    string GeneratePublicApplicationToken(
        Guid applicationId,
        Guid projectId,
        Guid credentialId);

    string GenerateAccessToken(
        Guid accountId,
        Guid userId,
        Guid applicationId,
        Guid projectId,
        string securityStamp,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);

    string GenerateRefreshToken();
}
