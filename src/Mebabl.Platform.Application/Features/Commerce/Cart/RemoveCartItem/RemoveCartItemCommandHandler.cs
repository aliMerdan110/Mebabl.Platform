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

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var item = await _db.CartItems
            .Include(x => x.Cart)
            .FirstOrDefaultAsync(x =>
                x.Id == request.CartItemId &&
                x.Cart.ApplicationId == applicationId &&
                x.Cart.UserId == userId,
                cancellationToken);

        // لا نكشف وجود عنصر يملكه مستخدم آخر.
        if (item is null)
            return false;

        _db.CartItems.Remove(item);

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}