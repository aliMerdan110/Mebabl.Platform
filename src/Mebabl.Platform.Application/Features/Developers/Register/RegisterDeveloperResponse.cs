namespace Mebabl.Platform.Application.Features.Developers.Register;

public sealed record RegisterDeveloperResponse(
    string AccessToken,
    string RefreshToken);