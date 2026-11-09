using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Commerce.Orders.GetOrder;

namespace Mebabl.Platform.Application.Features.Commerce.Orders.ListOrders;

public sealed class ListOrdersQueryHandler
    : IRequestHandler<ListOrdersQuery, IReadOnlyList<OrderDto>>
{
    private const int MaxPage = 100_000;
    private const int MaxPageSize = 100;

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
        if (!_currentApplication.IsAuthenticated ||
            _currentApplication.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The current application is not authenticated.");
        }

        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The current user is not authenticated.");
        }

        var page = Math.Clamp(request.Page, 1, MaxPage);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        return await _db.Orders
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
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