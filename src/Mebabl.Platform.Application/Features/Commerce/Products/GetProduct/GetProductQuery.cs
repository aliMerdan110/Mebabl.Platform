using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Products.GetProduct;

public sealed record GetProductQuery(Guid ProductId)
    : IRequest<ProductDto?>;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    IReadOnlyList<ProductImageDto> Images
);

public sealed record ProductImageDto(
    Guid Id,
    Guid StorageFileId,
    int Order
);