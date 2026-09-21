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
    private readonly JwtOptions _options;

    public JwtTokenGenerator(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    // مولّد مركزي لجميع رموز الهوية والصلاحيات في المنصة.
    public string GenerateDeveloperToken(
        Guid developerId)
    {
        var claims = new[]
        {
            new Claim(
                "developerId",
                developerId.ToString()),

            new Claim(
                "type",
                "developer")
        };

        return GenerateToken(
            claims,
            TimeSpan.FromMinutes(
                _options.ExpiryMinutes));
    }

    public string GenerateApplicationToken(
        Guid applicationId,
        Guid credentialId)
    {
        var claims = new[]
        {
            new Claim(
                "applicationId",
                applicationId.ToString()),

            new Claim(
                "credentialId",
                credentialId.ToString()),

            new Claim(
                "type",
                "application")
        };

        return GenerateToken(
            claims,
            TimeSpan.FromHours(1));
    }

    public string GenerateAccessToken(
        Guid accountId,
        Guid userId,
        Guid applicationId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        var claims = new List<Claim>
        {
            new Claim(
                "accountId",
                accountId.ToString()),

            new Claim(
                "userId",
                userId.ToString()),

            new Claim(
                "applicationId",
                applicationId.ToString()),

            new Claim(
                "type",
                "user"),

            new Claim(
                JwtRegisteredClaimNames.Sub,
                userId.ToString())
        };

        foreach (var role in roles)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }
        }

        foreach (var permission in permissions)
        {
            if (!string.IsNullOrWhiteSpace(permission))
            {
                claims.Add(
                    new Claim(
                        "permission",
                        permission));
            }
        }

        return GenerateToken(
            claims,
            TimeSpan.FromMinutes(
                _options.ExpiryMinutes));
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }

    private string GenerateToken(
        IEnumerable<Claim> claims,
        TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _options.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}