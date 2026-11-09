using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.GetCart;

public sealed class GetCartQueryHandler
    : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetCartQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(
        GetCartQuery request,
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

        var cart = await _db.Carts
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.UserId == userId,
                cancellationToken);

        if (cart is null)
        {
            return new CartDto(
                Guid.Empty,
                Array.Empty<CartItemDto>(),
                0);
        }

        var items = cart.Items
            .Where(x =>
                x.Product.ApplicationId == applicationId &&
                !x.Product.IsDeleted)
            .Select(x => new CartItemDto(
                x.Id,
                x.ProductId,
                x.Product.Name,
                x.Product.Price,
                x.Quantity,
                x.Product.Price * x.Quantity))
            .ToList();

        return new CartDto(
            cart.Id,
            items,
            items.Sum(x => x.TotalPrice));
    }
}