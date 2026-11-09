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
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        if (!_currentApplication.IsAuthenticated ||
            !_currentUser.IsAuthenticated ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        if (request.OrderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(request.OrderId));
        }

        return await _db.Orders
            .AsNoTracking()
            .Where(x =>
                x.Id == request.OrderId &&
                x.ApplicationId == applicationId &&
                x.UserId == userId)
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
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}