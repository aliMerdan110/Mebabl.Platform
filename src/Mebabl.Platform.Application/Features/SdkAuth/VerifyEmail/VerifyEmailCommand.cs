using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.VerifyEmail;

public sealed record VerifyEmailCommand(
    string Token
) : IRequest<VerifyEmailResponse>;