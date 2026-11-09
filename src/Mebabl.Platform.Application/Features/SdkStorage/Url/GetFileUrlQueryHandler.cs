using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkStorage.Url;

public sealed class GetFileUrlQueryHandler
    : IRequestHandler<GetFileUrlQuery, string>
{
    private readonly IApplicationDbContext _db;

    public GetFileUrlQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> Handle(
        GetFileUrlQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ApplicationId == Guid.Empty ||
            request.UserId == Guid.Empty ||
            request.FileId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Valid application, user and file identifiers are required.");
        }

        var file = await _db.StoredFiles
            .AsNoTracking()
            .Where(x =>
                x.Id == request.FileId &&
                x.ApplicationId == request.ApplicationId &&
                (x.IsPublic || x.UserId == request.UserId))
            .Select(x => new
            {
                x.Id
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (file is null)
        {
            throw new KeyNotFoundException("File not found.");
        }

        return $"/api/sdk/storage/{file.Id}/content";
    }
}