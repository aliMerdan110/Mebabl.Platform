using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.ForgotPassword;

public sealed record SdkForgotPasswordCommand(
    string Email
) : IRequest<SdkForgotPasswordResponse>;

public sealed record SdkForgotPasswordResponse(
    string Message
);