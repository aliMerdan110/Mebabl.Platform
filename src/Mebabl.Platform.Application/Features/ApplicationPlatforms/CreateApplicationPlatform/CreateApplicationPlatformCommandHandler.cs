using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Applications;

namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.CreateApplicationPlatform;

public sealed class CreateApplicationPlatformCommandHandler
    : IRequestHandler<
        CreateApplicationPlatformCommand,
        CreateApplicationPlatformResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateApplicationPlatformCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateApplicationPlatformResponse> Handle(
        CreateApplicationPlatformCommand request,
        CancellationToken cancellationToken)
    {
        var platform = request.Platform
            .Trim()
            .ToLowerInvariant();

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(
                x => x.Id == request.ApplicationId,
                cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        var exists = await _dbContext.ApplicationPlatforms
            .AnyAsync(
                x =>
                    x.ApplicationId == request.ApplicationId &&
                    x.Platform == platform,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"The {platform} platform is already registered for this application.");
        }

        var entity = new ApplicationPlatform
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            Platform = platform,
            Nickname = request.Nickname?.Trim(),
            PackageName = request.PackageName?.Trim(),
            BundleId = request.BundleId?.Trim(),
            Domain = request.Domain?.Trim(),
            IsActive = true
        };

        _dbContext.ApplicationPlatforms.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateApplicationPlatformResponse(
            entity.Id,
            entity.ApplicationId,
            entity.Platform,
            entity.Nickname,
            entity.PackageName,
            entity.BundleId,
            entity.Domain,
            entity.IsActive);
    }
}