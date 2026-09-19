using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Products.ListProducts;

public sealed record ListProductsQuery(
    int Page = 1,
    int PageSize = 50
) : IRequest<IReadOnlyList<ProductListItemDto>>;

public sealed record ProductListItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    Guid? PrimaryStorageFileId
);