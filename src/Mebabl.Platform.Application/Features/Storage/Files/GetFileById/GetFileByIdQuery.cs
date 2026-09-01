using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Files.GetFileById;

public sealed record GetFileByIdQuery(
    Guid Id
) : IRequest<GetFileByIdResponse>;