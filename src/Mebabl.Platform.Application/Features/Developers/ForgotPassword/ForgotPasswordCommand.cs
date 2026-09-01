
using MediatR;

namespace Mebabl.Platform.Application.Features.Developers.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email
) : IRequest<ForgotPasswordResponse>;

public sealed record ForgotPasswordResponse(
    string Message
);
