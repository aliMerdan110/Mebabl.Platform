using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Files.UploadFile;

public sealed record UploadFileCommand(
    Guid BucketId,
    string FileName,
    string ContentType,
    long Length,
    Stream Content
) : IRequest<UploadFileResponse>;