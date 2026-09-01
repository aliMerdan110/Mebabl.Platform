using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.Logout;

public sealed record SdkLogoutCommand(
    string RefreshToken)
    : IRequest;