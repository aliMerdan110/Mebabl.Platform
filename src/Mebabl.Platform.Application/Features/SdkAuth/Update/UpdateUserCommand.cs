using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.Update;

public sealed record UpdateUserCommand(
    Guid Id,
    string Username,
    bool IsActive)
    : IRequest;