
using MediatR;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.CreateProject;

public sealed record CreateProjectCommand(
    string Name,
    string Code,
    string? Description) : IRequest<ProjectDto>;
