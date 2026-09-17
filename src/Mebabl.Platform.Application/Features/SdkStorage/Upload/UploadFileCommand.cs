using MediatR;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Upload;

public sealed record UploadFileCommand(
    Guid ApplicationId,
    Guid? UserId,
    Stream Content,
    string FileName,
    string ContentType,
    string Path,
    bool IsPublic
) : IRequest<StoredFile>;
