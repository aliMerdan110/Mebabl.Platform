using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.UpdateApplication;

public sealed record UpdateApplicationCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain)
    : IRequest;