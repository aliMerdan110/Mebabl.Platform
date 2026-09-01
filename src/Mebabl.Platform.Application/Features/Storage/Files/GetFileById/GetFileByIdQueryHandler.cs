using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Storage.Files.GetFileById;

public sealed class GetFileByIdQueryHandler
    : IRequestHandler<GetFileByIdQuery, GetFileByIdResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public GetFileByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<GetFileByIdResponse> Handle(
        GetFileByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var file = await _context.StoredFiles
            .AsNoTracking()
            .Include(x => x.Bucket)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.Bucket.ApplicationId == _currentApplication.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (file is null)
            throw new Exception("File not found.");

        return new GetFileByIdResponse(
            file.Id,
            file.BucketId,
            file.Key,
            file.FileName,
            file.ContentType,
            file.Extension,
            file.Size,
            file.Hash,
            file.Version,
            file.CreatedAt);
    }
}