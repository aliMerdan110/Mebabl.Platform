using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive
) : IRequest<Guid>;