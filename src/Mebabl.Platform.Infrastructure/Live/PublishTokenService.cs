// Infrastructure/Live/PublishTokenService.cs

using System.Security.Cryptography;
using System.Text;
using Mebabl.Platform.Application.Services.Live;

namespace Mebabl.Platform.Infrastructure.Live;

public sealed class PublishTokenService : IPublishTokenService
{
    public string GenerateToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token)));
    }

    public bool VerifyToken(
        string token,
        string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(tokenHash))
        {
            return false;
        }

        byte[] suppliedHash;
        byte[] storedHash;

        try
        {
            suppliedHash = Convert.FromHexString(
                HashToken(token));

            storedHash = Convert.FromHexString(
                tokenHash);
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            suppliedHash,
            storedHash);
    }
}