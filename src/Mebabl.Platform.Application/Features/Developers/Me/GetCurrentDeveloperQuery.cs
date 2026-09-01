using MediatR;

namespace Mebabl.Platform.Application.Features.Developers.Me;

public sealed record GetCurrentDeveloperQuery
    : IRequest<GetCurrentDeveloperResponse>;