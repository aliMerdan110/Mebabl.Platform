using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Orders.GetOrder;

public sealed class GetOrderQueryHandler
    : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetOrderQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<OrderDto?> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Orders
            .AsNoTracking()
            .Where(x =>
                x.Id == request.OrderId &&
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.UserId == _currentUser.UserId)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}