using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Commerce;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.AddCartItem;

public sealed class AddCartItemCommandHandler
    : IRequestHandler<AddCartItemCommand, Guid>
{
    private const int MaximumQuantityPerItem = 1000;

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

        if (!_currentApplication.IsAuthenticated ||
            !_currentUser.IsAuthenticated ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        if (request.ProductId == Guid.Empty)
            throw new ArgumentException("Product ID is required.");

        if (request.Quantity < 1 ||
            request.Quantity > MaximumQuantityPerItem)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Quantity),
                $"Quantity must be between 1 and {MaximumQuantityPerItem}.");
        }

        var product = await _db.Products
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ProductId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    !x.IsDeleted,
                cancellationToken);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        var cart = await _db.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.UserId == userId,
                cancellationToken);

        var existingItem = cart?.Items
            .FirstOrDefault(x => x.ProductId == product.Id);

        var currentQuantity = existingItem?.Quantity ?? 0;

        if (currentQuantity > MaximumQuantityPerItem - request.Quantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Quantity),
                "The resulting cart quantity is too large.");
        }

        var newQuantity = currentQuantity + request.Quantity;

        if (newQuantity > product.StockQuantity)
        {
            throw new InvalidOperationException(
                "Requested quantity exceeds available stock.");
        }

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

        if (existingItem is not null)
        {
            existingItem.Quantity = newQuantity;
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