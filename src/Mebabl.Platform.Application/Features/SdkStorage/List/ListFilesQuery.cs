using MediatR;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.List;

public sealed record ListFilesQuery(
    string? Path) : IRequest<IReadOnlyList<StorageFileDto>>;