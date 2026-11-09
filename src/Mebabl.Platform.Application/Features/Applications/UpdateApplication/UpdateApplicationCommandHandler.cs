using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.UpdateApplication;

public sealed class UpdateApplicationHandler
    : IRequestHandler<UpdateApplicationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public UpdateApplicationHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        UpdateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Application name is required.",
                nameof(request.Name));
        }

        if (name.Length > 200)
        {
            throw new ArgumentException(
                "Application name cannot exceed 200 characters.",
                nameof(request.Name));
        }

        if (request.Description?.Length > 2000)
        {
            throw new ArgumentException(
                "Description cannot exceed 2000 characters.",
                nameof(request.Description));
        }

        var application = await _dbContext.Applications
            .Include(x => x.ProjectPlatform)
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.Project.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        var platform = application.ProjectPlatform.Platform
            .Trim()
            .ToLowerInvariant();

        var nickname = Normalize(request.Nickname);
        var packageName = Normalize(request.PackageName);
        var bundleId = Normalize(request.BundleId);
        var domain = Normalize(request.Domain);
        var description = Normalize(request.Description);

        ValidateLength(nickname, 100, nameof(request.Nickname));
        ValidateLength(packageName, 255, nameof(request.PackageName));
        ValidateLength(bundleId, 255, nameof(request.BundleId));
        ValidateLength(domain, 255, nameof(request.Domain));

        switch (platform)
        {
            case "android":
                if (string.IsNullOrWhiteSpace(packageName))
                {
                    throw new ArgumentException(
                        "Package name is required for Android applications.",
                        nameof(request.PackageName));
                }

                bundleId = null;
                domain = null;
                break;

            case "ios":
                if (string.IsNullOrWhiteSpace(bundleId))
                {
                    throw new ArgumentException(
                        "Bundle ID is required for iOS applications.",
                        nameof(request.BundleId));
                }

                packageName = null;
                domain = null;
                break;

            case "web":
                if (string.IsNullOrWhiteSpace(domain))
                {
                    throw new ArgumentException(
                        "Domain is required for web applications.",
                        nameof(request.Domain));
                }

                packageName = null;
                bundleId = null;
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported application platform: {platform}.");
        }

        application.Name = name;
        application.Description = description;

        application.ProjectPlatform.Nickname = nickname;
        application.ProjectPlatform.PackageName = packageName;
        application.ProjectPlatform.BundleId = bundleId;
        application.ProjectPlatform.Domain = domain;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static void ValidateLength(
        string? value,
        int maxLength,
        string parameterName)
    {
        if (value?.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }
    }
}