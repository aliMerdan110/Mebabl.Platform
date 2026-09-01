using MediatR;

namespace Mebabl.Platform.Application.Features.Users.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Username,
    string Password
) : IRequest<RegisterUserResponse>;