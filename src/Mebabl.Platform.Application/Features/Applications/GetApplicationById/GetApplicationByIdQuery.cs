using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.GetApplicationById;

public sealed record GetApplicationByIdQuery(
    Guid Id)
    : IRequest<GetApplicationByIdResponse>;