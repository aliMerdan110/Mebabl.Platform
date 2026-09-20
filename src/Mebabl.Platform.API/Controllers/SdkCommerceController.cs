using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Commerce.Cart.AddCartItem;
using Mebabl.Platform.Application.Features.Commerce.Cart.GetCart;
using Mebabl.Platform.Application.Features.Commerce.Cart.RemoveCartItem;
using Mebabl.Platform.Application.Features.Commerce.Cart.UpdateCartItem;

using Mebabl.Platform.Application.Features.Commerce.Orders.CreateOrder;
using Mebabl.Platform.Application.Features.Commerce.Orders.GetOrder;
using Mebabl.Platform.Application.Features.Commerce.Orders.ListOrders;

using Mebabl.Platform.Application.Features.Commerce.Products.CreateProduct;
using Mebabl.Platform.Application.Features.Commerce.Products.DeleteProduct;
using Mebabl.Platform.Application.Features.Commerce.Products.GetProduct;
using Mebabl.Platform.Application.Features.Commerce.Products.ListProducts;
using Mebabl.Platform.Application.Features.Commerce.Products.UpdateProduct;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize(Policy = "ApplicationUser")]
[Route("api/sdk/commerce")]
public sealed class SdkCommerceController : ControllerBase
{
    private readonly ISender _sender;

    public SdkCommerceController(ISender sender)
    {
        _sender = sender;
    }

    // =========================================================
    // Products
    // =========================================================

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            command,
            cancellationToken);

        return Ok(new { id });
    }

    [HttpGet("products/{productId:guid}")]
    public async Task<IActionResult> GetProduct(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetProductQuery(productId),
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> ListProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListProductsQuery(
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("products/{productId:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateProductCommand(
                productId,
                request.Name,
                request.Description,
                request.Price,
                request.Currency,
                request.StockQuantity,
                request.IsActive),
            cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("products/{productId:guid}")]
    public async Task<IActionResult> DeleteProduct(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new DeleteProductCommand(productId),
            cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }

    // =========================================================
    // Cart
    // =========================================================

    [HttpGet("cart")]
    public async Task<IActionResult> GetCart(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCartQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("cart/items")]
    public async Task<IActionResult> AddCartItem(
        [FromBody] AddCartItemCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            command,
            cancellationToken);

        return Ok(new { id });
    }

    [HttpPut("cart/items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateCartItem(
        Guid cartItemId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateCartItemCommand(
                cartItemId,
                request.Quantity),
            cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("cart/items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveCartItem(
        Guid cartItemId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RemoveCartItemCommand(cartItemId),
            cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }

    // =========================================================
    // Orders
    // =========================================================

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder(
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            new CreateOrderCommand(),
            cancellationToken);

        return Ok(new { id });
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<IActionResult> GetOrder(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetOrderQuery(orderId),
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> ListOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListOrdersQuery(
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }

    // =========================================================
    // Requests
    // =========================================================

    public sealed record UpdateProductRequest(
        string Name,
        string? Description,
        decimal Price,
        string Currency,
        int StockQuantity,
        bool IsActive);

    public sealed record UpdateCartItemRequest(
        int Quantity);
}