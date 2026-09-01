using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Username,
    string Password
) : IRequest<RegisterUserResponse>;