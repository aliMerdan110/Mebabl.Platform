using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Commerce;

namespace Mebabl.Platform.Application.Features.Commerce.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public CreateOrderCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _db.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.UserId == _currentUser.UserId,
                cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
                throw new InvalidOperationException(
                    $"Product '{item.Product.Name}' is unavailable.");

            if (item.Quantity > item.Product.StockQuantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{item.Product.Name}'.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ApplicationId = _currentApplication.ApplicationId,
            UserId = _currentUser.UserId,
            Status = "Pending",
            Currency = cart.Items.First().Product.Currency
        };

        foreach (var cartItem in cart.Items)
        {
            var total = cartItem.Product.Price * cartItem.Quantity;

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                UnitPrice = cartItem.Product.Price,
                Quantity = cartItem.Quantity,
                TotalPrice = total
            });

            cartItem.Product.StockQuantity -= cartItem.Quantity;
        }

        order.TotalAmount = order.Items.Sum(x => x.TotalPrice);

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);

        await _db.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}