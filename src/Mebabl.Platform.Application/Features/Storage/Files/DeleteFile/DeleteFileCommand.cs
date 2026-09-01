using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Files.DeleteFile;

public sealed record DeleteFileCommand(
    Guid Id
) : IRequest;