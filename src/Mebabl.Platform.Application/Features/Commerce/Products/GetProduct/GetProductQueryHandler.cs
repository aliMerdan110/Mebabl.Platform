using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Products.GetProduct;

public sealed class GetProductQueryHandler
    : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public GetProductQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<ProductDto?> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(x =>
                x.Id == request.ProductId &&
                x.ApplicationId == _currentApplication.ApplicationId)
            .Select(x => new ProductDto(
                x.Id,
                x.Name,
                x.Description,
                x.Price,
                x.Currency,
                x.StockQuantity,
                x.IsActive,
                x.Images
                    .OrderBy(i => i.Order)
                    .Select(i => new ProductImageDto(
                        i.Id,
                        i.StorageFileId,
                        i.Order))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}