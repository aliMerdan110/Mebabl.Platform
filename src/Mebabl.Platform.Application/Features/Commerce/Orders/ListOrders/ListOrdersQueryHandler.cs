using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Commerce.Orders.GetOrder;

namespace Mebabl.Platform.Application.Features.Commerce.Orders.ListOrders;

public sealed class ListOrdersQueryHandler
    : IRequestHandler<ListOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public ListOrdersQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(
        ListOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return await _db.Orders
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.UserId == _currentUser.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrderDto(
                x.Id,
                x.Status,
                x.TotalAmount,
                x.Currency,
                x.CreatedAt,
                x.Items
                    .OrderBy(i => i.CreatedAt)
                    .Select(i => new OrderItemDto(
                        i.ProductId,
                        i.ProductName,
                        i.UnitPrice,
                        i.Quantity,
                        i.TotalPrice))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}