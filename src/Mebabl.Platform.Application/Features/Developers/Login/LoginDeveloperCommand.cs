using MediatR;

namespace Mebabl.Platform.Application.Features.Developers.Login;

public sealed record LoginDeveloperCommand(
    string Email,
    string Password
) : IRequest<LoginDeveloperResponse>;