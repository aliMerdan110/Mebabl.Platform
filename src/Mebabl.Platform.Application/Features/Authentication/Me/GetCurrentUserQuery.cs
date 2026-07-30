using MediatR;
using Mebabl.Platform.Application.Features.Authentication.DTOs;

namespace Mebabl.Platform.Application.Features.Authentication.Me;

public sealed record GetCurrentUserQuery
    : IRequest<CurrentUserResponse>;