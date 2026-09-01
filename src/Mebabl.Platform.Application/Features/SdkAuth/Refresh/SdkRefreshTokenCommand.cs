using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.Refresh;

public sealed record SdkRefreshTokenCommand(
    string RefreshToken)
    : IRequest<SdkRefreshTokenResponse>;