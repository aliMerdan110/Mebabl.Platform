using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Cart.AddCartItem;

public sealed record AddCartItemCommand(
    Guid ProductId,
    int Quantity) : IRequest<Guid>;