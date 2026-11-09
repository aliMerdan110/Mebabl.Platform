using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;

public sealed class CreateProjectPlatformFingerprintCommandHandler
    : IRequestHandler<
        CreateProjectPlatformFingerprintCommand,
        ProjectPlatformFingerprintDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public CreateProjectPlatformFingerprintCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectPlatformFingerprintDto> Handle(
        CreateProjectPlatformFingerprintCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var platform = await _dbContext.ProjectPlatforms
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.PlatformId &&
                    x.ProjectId == request.ProjectId &&
                    x.Project.DeveloperId == _currentDeveloper.DeveloperId &&
                    x.Project.IsActive &&
                    !x.Project.IsDeleted,
                cancellationToken);

        if (platform is null)
        {
            throw new KeyNotFoundException(
                "Project platform was not found.");
        }

        if (!string.Equals(
                platform.Platform,
                "android",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Fingerprints are only supported for Android platforms.");
        }

        var type = NormalizeType(request.FingerprintType);
        var value = NormalizeValue(request.Value, type);

        var exists = await _dbContext.ProjectPlatformFingerprints
            .AnyAsync(
                x =>
                    x.ProjectPlatformId == platform.Id &&
                    x.FingerprintType == type &&
                    x.Value == value,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "This fingerprint already exists.");
        }

        var fingerprint = new ProjectPlatformFingerprint
        {
            ProjectPlatformId = platform.Id,
            FingerprintType = type,
            Value = value
        };

        _dbContext.ProjectPlatformFingerprints.Add(fingerprint);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectPlatformFingerprintDto(
            fingerprint.Id,
            fingerprint.ProjectPlatformId,
            fingerprint.FingerprintType,
            FormatValue(fingerprint.Value),
            fingerprint.CreatedAt);
    }

    private static string NormalizeType(string? value)
    {
        var normalized = value?
            .Trim()
            .Replace("-", string.Empty)
            .ToUpperInvariant();

        return normalized switch
        {
            "SHA1" => "SHA1",
            "SHA256" => "SHA256",
            _ => throw new ArgumentException(
                "FingerprintType must be SHA-1 or SHA-256.",
                nameof(value))
        };
    }

    private static string NormalizeValue(string? value, string type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Fingerprint value is required.",
                nameof(value));
        }

        var normalized = value
            .Replace(":", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .Trim()
            .ToUpperInvariant();

        var expectedLength = type == "SHA1" ? 40 : 64;

        if (normalized.Length != expectedLength ||
            !Regex.IsMatch(normalized, @"\A[0-9A-F]+\z"))
        {
            throw new ArgumentException(
                $"Invalid {type} fingerprint. Expected {expectedLength} hexadecimal characters.",
                nameof(value));
        }

        return normalized;
    }

    private static string FormatValue(string value) =>
        string.Join(
            ":",
            Enumerable.Range(0, value.Length / 2)
                .Select(i => value.Substring(i * 2, 2)));
}