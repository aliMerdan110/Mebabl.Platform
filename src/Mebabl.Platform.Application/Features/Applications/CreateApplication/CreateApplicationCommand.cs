
using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.CreateApplication;

public sealed record CreateApplicationCommand(
    Guid ProjectId,
    string Name,
    string Code,
    string Platform,
    string? Description,
    string? Nickname = null,
    string? PackageName = null,
    string? BundleId = null,
    string? Domain = null
) : IRequest<CreateApplicationResponse>;
