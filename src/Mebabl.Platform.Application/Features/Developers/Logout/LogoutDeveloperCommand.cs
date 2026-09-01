using MediatR;

namespace Mebabl.Platform.Application.Features.Developers.Logout;

public sealed record LogoutDeveloperCommand(
    string RefreshToken)
    : IRequest;