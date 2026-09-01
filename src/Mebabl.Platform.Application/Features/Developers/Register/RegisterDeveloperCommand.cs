using MediatR;

namespace Mebabl.Platform.Application.Features.Developers.Register;

public sealed record RegisterDeveloperCommand(
    string DisplayName,
    string Email,
    string Password
) :IRequest<RegisterDeveloperResponse>;