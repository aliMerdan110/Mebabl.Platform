
namespace Mebabl.Platform.Application.Services.PasswordReset;

public interface IPasswordResetTokenService
{
    string GenerateToken();

    string HashToken(string token);
}
