using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.GetMyApplications;

public sealed record GetMyApplicationsQuery
    : IRequest<IReadOnlyList<ApplicationItemResponse>>;