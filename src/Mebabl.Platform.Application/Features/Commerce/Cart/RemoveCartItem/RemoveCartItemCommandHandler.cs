using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler
    : IRequestHandler<RemoveCartItemCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public RemoveCartItemCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(
        RemoveCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _db.CartItems
            .Include(x => x.Cart)
            .FirstOrDefaultAsync(x =>
                x.Id == request.CartItemId &&
                x.Cart.ApplicationId == _currentApplication.ApplicationId &&
                x.Cart.UserId == _currentUser.UserId,
                cancellationToken);

        if (item is null)
            return false;

        _db.CartItems.Remove(item);

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}