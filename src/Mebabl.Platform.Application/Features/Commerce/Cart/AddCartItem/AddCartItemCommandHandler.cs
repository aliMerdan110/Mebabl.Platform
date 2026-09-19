using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Commerce;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.AddCartItem;

public sealed class AddCartItemCommandHandler
    : IRequestHandler<AddCartItemCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public AddCartItemCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        AddCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var product = await _db.Products
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ProductId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive,
                cancellationToken);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        if (request.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        var cart = await _db.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.UserId == userId,
                cancellationToken);

        if (cart is null)
        {
            cart = new Mebabl.Platform.Domain.Entities.Commerce.Cart
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                UserId = userId
            };

            _db.Carts.Add(cart);
        }

        var existingItem = cart.Items
            .FirstOrDefault(x => x.ProductId == request.ProductId);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;

            await _db.SaveChangesAsync(cancellationToken);

            return existingItem.Id;
        }

        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = request.Quantity
        };

        cart.Items.Add(item);

        await _db.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}