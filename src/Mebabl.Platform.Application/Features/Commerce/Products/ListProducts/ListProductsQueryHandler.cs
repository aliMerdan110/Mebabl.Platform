using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Products.ListProducts;

public sealed class ListProductsQueryHandler
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public ListProductsQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<ProductListItemDto>> Handle(
        ListProductsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return await _db.Products
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductListItemDto(
                x.Id,
                x.Name,
                x.Description,
                x.Price,
                x.Currency,
                x.StockQuantity,
                x.IsActive,
                x.Images
                    .OrderBy(i => i.Order)
                    .Select(i => (Guid?)i.StorageFileId)
                    .FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);
    }
}