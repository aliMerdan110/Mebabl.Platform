using MediatR;

namespace Mebabl.Platform.Application.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(
    Guid Id)
    : IRequest<GetUserByIdResponse>;