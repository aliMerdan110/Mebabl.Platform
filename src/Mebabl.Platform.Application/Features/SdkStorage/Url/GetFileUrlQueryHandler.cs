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
        var file = await _db.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.FileId &&
                    x.ApplicationId == request.ApplicationId,
                cancellationToken);

        if (file is null)
            throw new KeyNotFoundException("File not found.");

        return file.IsPublic
            ? $"/api/sdk/storage/{file.Id}/content"
            : $"/api/sdk/storage/{file.Id}/content";
    }
}