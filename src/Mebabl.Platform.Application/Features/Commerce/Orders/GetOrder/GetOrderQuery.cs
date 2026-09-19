using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId)
    : IRequest<OrderDto?>;

public sealed record OrderDto(
    Guid Id,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items
);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);