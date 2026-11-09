using MediatR;

namespace Mebabl.Platform.Application.Features.SdkStorage.Delete;

public sealed record DeleteFileCommand(
    Guid ApplicationId,
    Guid UserId,
    Guid FileId
) : IRequest;