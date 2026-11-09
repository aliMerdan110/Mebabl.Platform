using MediatR;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.GetProjectSdkConfiguration;

public sealed record GetProjectSdkConfigurationQuery(
    Guid ProjectId)
    : IRequest<ProjectSdkConfigurationDto>;