
using MediatR;

namespace Mebabl.Platform.Application.Features.Developers.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword
) : IRequest<ResetPasswordResponse>;

public sealed record ResetPasswordResponse(
    string Message
);
