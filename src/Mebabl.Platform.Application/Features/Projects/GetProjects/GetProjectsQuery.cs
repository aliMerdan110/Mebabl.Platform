
using MediatR;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.GetProjects;

public sealed record GetProjectsQuery
    : IRequest<IReadOnlyList<ProjectDto>>;
