using MediatR;

namespace Mebabl.Platform.Application.Features.Users.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResponse>;