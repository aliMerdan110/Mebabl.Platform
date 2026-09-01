using MediatR;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;

public sealed record ToggleAuthProviderCommand(
    Guid ApplicationId,
    string Provider,
    bool IsEnabled
) : IRequest;