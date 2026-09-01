using MediatR;

namespace Mebabl.Platform.Application.Features.Developers.RefreshToken;

public sealed record RefreshDeveloperTokenCommand(
    string RefreshToken)
    : IRequest<RefreshDeveloperTokenResponse>;