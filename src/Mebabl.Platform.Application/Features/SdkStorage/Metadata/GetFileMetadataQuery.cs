using MediatR;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.Metadata;

public sealed record GetFileMetadataQuery(
    Guid FileId) : IRequest<StorageFileDto>;