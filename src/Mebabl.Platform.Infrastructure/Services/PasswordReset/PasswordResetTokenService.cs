
using System.Security.Cryptography;
using System.Text;
using Mebabl.Platform.Application.Services.PasswordReset;

namespace Mebabl.Platform.Infrastructure.Services.PasswordReset;

public sealed class PasswordResetTokenService
    : IPasswordResetTokenService
{
    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
