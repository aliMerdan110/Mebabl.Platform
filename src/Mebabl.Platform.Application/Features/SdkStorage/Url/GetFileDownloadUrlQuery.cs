
using MediatR;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.Url;

public sealed record GetFileDownloadUrlQuery(
Guid FileId
) : IRequest<StorageDownloadUrlDto>;