using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.GetCart;

public sealed record GetCartQuery : IRequest<CartDto>;

public sealed record CartDto(
    Guid Id,
    IReadOnlyList<CartItemDto> Items,
    decimal TotalAmount
);

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);