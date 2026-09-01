using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.GetUsers;

public sealed record GetUsersQuery
    : IRequest<IReadOnlyList<UserListItem>>;