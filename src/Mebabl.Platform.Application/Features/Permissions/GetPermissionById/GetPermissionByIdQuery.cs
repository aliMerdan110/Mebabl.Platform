using MediatR;

namespace Mebabl.Platform.Application.Features.Permissions.GetPermissionById;

public sealed record GetPermissionByIdQuery(Guid Id)
    : IRequest<GetPermissionByIdResponse>;