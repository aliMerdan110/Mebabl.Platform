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
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.UserId == userId,
                cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        foreach (var item in cart.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException(
                    "The cart contains an invalid quantity.");

            if (item.Product.ApplicationId != applicationId ||
                !item.Product.IsActive ||
                item.Product.IsDeleted)
            {
                throw new InvalidOperationException(
                    "The cart contains an unavailable product.");
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for '{item.Product.Name}'.");
            }
        }

        var currencies = cart.Items
            .Select(x => x.Product.Currency)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (currencies.Length != 1)
        {
            throw new InvalidOperationException(
                "All products in an order must use the same currency.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            UserId = userId,
            Status = "Pending",
            Currency = currencies[0]
        };

        foreach (var cartItem in cart.Items)
        {
            var product = cartItem.Product;
            var total = product.Price * cartItem.Quantity;

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
                TotalPrice = total
            });

            product.StockQuantity -= cartItem.Quantity;
        }

        order.TotalAmount = order.Items.Sum(x => x.TotalPrice);

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);

        // EF Core ينفذ تغييرات SaveChanges ضمن معاملة قاعدة بيانات واحدة.
        await _db.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}