using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.ChangePassword;

public sealed record SdkChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest;