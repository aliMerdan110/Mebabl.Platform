using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.ResetPassword;

public sealed record SdkResetPasswordCommand(
    string Token,
    string NewPassword
) : IRequest<SdkResetPasswordResponse>;

public sealed record SdkResetPasswordResponse(
    string Message
);