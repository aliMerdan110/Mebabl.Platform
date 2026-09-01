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

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    // =========================================================
    // Developer Token
    // =========================================================

    public string GenerateDeveloperToken(Guid developerId)
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

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    // =========================================================
    // Application Token
    // =========================================================

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

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    // =========================================================
    // User Access Token
    // =========================================================

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
            claims.Add(new Claim(
                ClaimTypes.Role,
                role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(
                "permission",
                permission));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    // =========================================================
    // Refresh Token
    // =========================================================

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }
}