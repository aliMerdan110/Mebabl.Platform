using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Commerce;

namespace Mebabl.Platform.Application.Features.Commerce.Products.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public CreateProductCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ApplicationId = _currentApplication.ApplicationId,
            OwnerId = _currentUser.UserId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive
        };

        _db.Products.Add(product);

        await _db.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}