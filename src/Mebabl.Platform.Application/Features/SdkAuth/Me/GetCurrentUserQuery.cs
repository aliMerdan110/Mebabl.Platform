using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.Me;

public sealed record GetCurrentUserQuery
    : IRequest<CurrentUserResponse>;