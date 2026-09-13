using MediatR;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.Upload;

public sealed record UploadFileCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long Size,
    string Path,
    bool IsPublic
) : IRequest<StorageFileDto>;