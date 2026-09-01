using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.ResendVerificationEmail;

public sealed record ResendVerificationEmailCommand
    : IRequest<ResendVerificationEmailResponse>;