using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive
) : IRequest<bool>;