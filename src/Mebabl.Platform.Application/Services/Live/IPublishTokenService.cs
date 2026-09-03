// Application/Services/Live/IPublishTokenService.cs

namespace Mebabl.Platform.Application.Services.Live;

public interface IPublishTokenService
{
    string GenerateToken();

    string HashToken(string token);

    bool VerifyToken(
        string token,
        string tokenHash);
}