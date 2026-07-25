namespace Mebabl.Platform.Application.Services.Authentication;

public interface IAuthenticationService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);
}