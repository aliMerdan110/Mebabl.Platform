using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler
    : IRequestHandler<UpdateCartItemCommand, bool>
{
    private const int MaxQuantity = 1000;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public UpdateCartItemCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(
        UpdateCartItemCommand request,
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

        if (request.CartItemId == Guid.Empty)
            return false;

        if (request.Quantity < 0 || request.Quantity > MaxQuantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Quantity),
                $"Quantity must be between 0 and {MaxQuantity}.");
        }

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var item = await _db.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Id == request.CartItemId &&
                x.Cart.ApplicationId == applicationId &&
                x.Cart.UserId == userId,
                cancellationToken);

        if (item is null)
            return false;

        // تأكد من أن المنتج تابع للتطبيق نفسه.
        if (item.Product.ApplicationId != applicationId)
            return false;

        // الكمية صفر تعني حذف العنصر من السلة.
        if (request.Quantity == 0)
        {
            _db.CartItems.Remove(item);
        }
        else
        {
            if (item.Product.IsDeleted || !item.Product.IsActive)
            {
                throw new InvalidOperationException(
                    "The product is no longer available.");
            }

            if (request.Quantity > item.Product.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Insufficient stock.");
            }

            item.Quantity = request.Quantity;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}