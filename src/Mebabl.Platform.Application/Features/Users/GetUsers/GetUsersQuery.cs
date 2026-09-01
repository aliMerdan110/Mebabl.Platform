using MediatR;

namespace Mebabl.Platform.Application.Features.Users.GetUsers;

public sealed record GetUsersQuery
    : IRequest<IReadOnlyList<UserListItem>>;