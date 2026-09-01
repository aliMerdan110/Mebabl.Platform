using MediatR;

namespace Mebabl.Platform.Application.Features.Users.Update;

public sealed record UpdateUserCommand(
    Guid Id,
    string Username,
    bool IsActive)
    : IRequest;