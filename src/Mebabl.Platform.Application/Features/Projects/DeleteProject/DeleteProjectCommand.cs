
using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.DeleteProject;

public sealed record DeleteProjectCommand(
    Guid ProjectId) : IRequest;
