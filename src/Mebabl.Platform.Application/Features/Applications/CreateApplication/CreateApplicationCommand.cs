using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.CreateApplication;

public sealed record CreateApplicationCommand(
    string Name,
    string Code,
    string? Description
) : IRequest<CreateApplicationResponse>;