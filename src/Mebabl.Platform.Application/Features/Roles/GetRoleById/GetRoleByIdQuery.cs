using MediatR;

namespace Mebabl.Platform.Application.Features.Roles.GetRoleById;

public sealed record GetRoleByIdQuery(Guid Id)
    : IRequest<GetRoleByIdResponse>;