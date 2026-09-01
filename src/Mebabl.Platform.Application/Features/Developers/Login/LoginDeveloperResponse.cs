namespace Mebabl.Platform.Application.Features.Developers.Login;

public sealed record LoginDeveloperResponse(
    string AccessToken,
    string RefreshToken);