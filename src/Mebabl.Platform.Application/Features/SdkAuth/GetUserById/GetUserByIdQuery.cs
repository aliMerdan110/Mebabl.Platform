using MediatR;

namespace Mebabl.Platform.Application.Features.SdkAuth.GetUserById;

public sealed record GetUserByIdQuery(
    Guid Id)
    : IRequest<GetUserByIdResponse>;