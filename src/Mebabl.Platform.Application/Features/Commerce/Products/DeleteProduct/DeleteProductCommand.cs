using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId)
    : IRequest<bool>;