using MediatR;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;

public sealed record GetAuthProvidersQuery(
    Guid ApplicationId
) : IRequest<IReadOnlyList<AuthProviderResponse>>;