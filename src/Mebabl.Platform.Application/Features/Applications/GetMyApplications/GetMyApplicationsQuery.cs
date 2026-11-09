using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.GetMyApplications;

public sealed record GetMyApplicationsQuery(
    Guid? ProjectId = null)
    : IRequest<IReadOnlyList<ApplicationItemResponse>>;