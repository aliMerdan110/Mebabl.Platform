using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.Users.CreateApplicationUser;

public sealed record CreateApplicationUserCommand(
    Guid ApplicationId,
    string Email,
    string Password,
    string Username,
    string DisplayName
) : IRequest<CreateApplicationUserResponse>;