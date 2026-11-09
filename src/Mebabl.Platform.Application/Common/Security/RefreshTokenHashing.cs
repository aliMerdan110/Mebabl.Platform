using System.Security.Cryptography;
using System.Text;

namespace Mebabl.Platform.Application.Common.Security;

public static class RefreshTokenHashing
{
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token)));
    }
}