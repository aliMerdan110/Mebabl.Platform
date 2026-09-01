namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;

public sealed record AuthProviderResponse(
    Guid Id,
    string Provider,
    bool IsEnabled);