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
        var cart = await _db.Carts
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.UserId == _currentUser.UserId,
                cancellationToken);

        if (cart is null)
        {
            return new CartDto(
                Guid.Empty,
                Array.Empty<CartItemDto>(),
                0);
        }

        var items = cart.Items
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