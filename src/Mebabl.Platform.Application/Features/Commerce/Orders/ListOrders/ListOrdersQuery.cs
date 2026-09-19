using MediatR;
using Mebabl.Platform.Application.Features.Commerce.Orders.GetOrder;

namespace Mebabl.Platform.Application.Features.Commerce.Orders.ListOrders;

public sealed record ListOrdersQuery(
    int Page = 1,
    int PageSize = 50
) : IRequest<IReadOnlyList<OrderDto>>;