using MediatR;

namespace Mebabl.Platform.Application.Features.Commerce.Orders.CreateOrder;

public sealed record CreateOrderCommand : IRequest<Guid>;