using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResponse>;