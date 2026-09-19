using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.UpdateCartItem;

public sealed record UpdateCartItemCommand(
    Guid CartItemId,
    int Quantity
) : IRequest<bool>;