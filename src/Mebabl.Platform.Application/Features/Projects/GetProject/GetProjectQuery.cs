
using MediatR;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.GetProject;

public sealed record GetProjectQuery(
    Guid ProjectId) : IRequest<ProjectDto>;
