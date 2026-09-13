using MediatR;

namespace Mebabl.Platform.Application.Features.SdkStorage.Delete;

public sealed record DeleteFileCommand(
    Guid FileId) : IRequest;