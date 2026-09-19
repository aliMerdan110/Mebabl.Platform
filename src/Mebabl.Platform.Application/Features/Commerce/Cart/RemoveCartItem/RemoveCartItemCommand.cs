using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid CartItemId)
    : IRequest<bool>;