using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler
    : IRequestHandler<UpdateCartItemCommand, bool>
{
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
        var item = await _db.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Id == request.CartItemId &&
                x.Cart.ApplicationId == _currentApplication.ApplicationId &&
                x.Cart.UserId == _currentUser.UserId,
                cancellationToken);

        if (item is null)
            return false;

        if (request.Quantity <= 0)
        {
            _db.CartItems.Remove(item);
        }
        else
        {
            if (request.Quantity > item.Product.StockQuantity)
                throw new InvalidOperationException("Insufficient stock.");

            item.Quantity = request.Quantity;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}