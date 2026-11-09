using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Mebabl.Platform.Application.Services.Jwt;

namespace Mebabl.Platform.Infrastructure.Authentication.Jwt;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private static readonly TimeSpan PublicApplicationTokenLifetime =
        TimeSpan.FromMinutes(10);

    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value
            ?? throw new InvalidOperationException(
                "JWT configuration is required.");

        if (string.IsNullOrWhiteSpace(_options.Secret) ||
            Encoding.UTF8.GetByteCount(_options.Secret) < 32)
        {
            throw new InvalidOperationException(
                "JWT signing secret must contain at least 32 UTF-8 bytes.");
        }

        if (string.IsNullOrWhiteSpace(_options.Issuer))
            throw new InvalidOperationException("JWT issuer must be configured.");

        if (string.IsNullOrWhiteSpace(_options.Audience))
            throw new InvalidOperationException("JWT audience must be configured.");

        if (_options.ExpiryMinutes <= 0)
            throw new InvalidOperationException("JWT expiry must be greater than zero.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        _signingCredentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);
    }

    public string GenerateDeveloperToken(Guid developerId)
    {
        ValidateIdentifier(developerId, nameof(developerId));

        return GenerateToken(
        [
            new Claim("developerId", developerId.ToString("D")),
            new Claim("type", "developer")
        ],
        TimeSpan.FromMinutes(_options.ExpiryMinutes));
    }

    /// <summary>
    /// Creates a trusted server token scoped to one application and its project.
    /// The project claim is validated against the application's database relationship
    /// on every authenticated request; it is not accepted as an authority by itself.
    /// </summary>
    public string GenerateApplicationToken(
        Guid applicationId,
        Guid projectId,
        Guid credentialId)
    {
        ValidateApplicationIdentifiers(applicationId, projectId, credentialId);

        return GenerateToken(
        [
            new Claim("applicationId", applicationId.ToString("D")),
            new Claim("projectId", projectId.ToString("D")),
            new Claim("credentialId", credentialId.ToString("D")),
            new Claim("type", "application"),
            new Claim("authLevel", "secret")
        ],
        TimeSpan.FromHours(1));
    }

    /// <summary>
    /// Creates a short-lived public application token. It identifies the project
    /// and calling application but grants no database permission on its own.
    /// </summary>
    public string GeneratePublicApplicationToken(
        Guid applicationId,
        Guid projectId,
        Guid credentialId)
    {
        ValidateApplicationIdentifiers(applicationId, projectId, credentialId);

        return GenerateToken(
        [
            new Claim("applicationId", applicationId.ToString("D")),
            new Claim("projectId", projectId.ToString("D")),
            new Claim("credentialId", credentialId.ToString("D")),
            new Claim("type", "application_public"),
            new Claim("scope", "public"),
            new Claim("authLevel", "public")
        ],
        PublicApplicationTokenLifetime);
    }

    public string GenerateAccessToken(
        Guid accountId,
        Guid userId,
        Guid applicationId,
        Guid projectId,
        string securityStamp,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        ValidateIdentifier(accountId, nameof(accountId));
        ValidateIdentifier(userId, nameof(userId));
        ValidateIdentifier(applicationId, nameof(applicationId));
        ValidateIdentifier(projectId, nameof(projectId));

        ArgumentException.ThrowIfNullOrWhiteSpace(securityStamp);
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(permissions);

        var claims = new List<Claim>
        {
            new("accountId", accountId.ToString("D")),
            new("userId", userId.ToString("D")),
            new("applicationId", applicationId.ToString("D")),
            new("projectId", projectId.ToString("D")),
            new("type", "user"),
            new(JwtRegisteredClaimNames.Sub, userId.ToString("D")),
            new("securityStamp", securityStamp)
        };

        foreach (var role in roles
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Select(static value => value.Trim())
                     .Distinct(StringComparer.Ordinal))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Select(static value => value.Trim())
                     .Distinct(StringComparer.Ordinal))
        {
            claims.Add(new Claim("permission", permission));
        }

        return GenerateToken(
            claims,
            TimeSpan.FromMinutes(_options.ExpiryMinutes));
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private string GenerateToken(
        IEnumerable<Claim> claims,
        TimeSpan lifetime)
    {
        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime));

        var now = DateTime.UtcNow;
        var tokenClaims = claims.ToList();
        tokenClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: tokenClaims,
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void ValidateApplicationIdentifiers(
        Guid applicationId,
        Guid projectId,
        Guid credentialId)
    {
        ValidateIdentifier(applicationId, nameof(applicationId));
        ValidateIdentifier(projectId, nameof(projectId));
        ValidateIdentifier(credentialId, nameof(credentialId));
    }

    private static void ValidateIdentifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Identifier cannot be empty.", parameterName);
    }
}
