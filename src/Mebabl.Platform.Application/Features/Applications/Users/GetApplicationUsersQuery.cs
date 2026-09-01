using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.Users;

public sealed record GetApplicationUsersQuery(
    Guid ApplicationId)
    : IRequest<IReadOnlyList<ApplicationUserDto>>;