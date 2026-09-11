using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

public sealed record UpdateMobileAppLinkCommand(
    Guid ApplicationId,
    Guid Id,
    string? AndroidPackageName,
    string? AndroidSha256CertificateFingerprint,
    string? IosBundleId,
    string? IosTeamId,
    bool IsActive
) : IRequest;

public sealed class UpdateMobileAppLinkCommandValidator
    : AbstractValidator<UpdateMobileAppLinkCommand>
{
    public UpdateMobileAppLinkCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.AndroidPackageName)
            .MaximumLength(255);

        RuleFor(x => x.AndroidSha256CertificateFingerprint)
            .MaximumLength(128);

        RuleFor(x => x.IosBundleId)
            .MaximumLength(255);

        RuleFor(x => x.IosTeamId)
            .MaximumLength(64);
    }
}

public sealed class UpdateMobileAppLinkCommandHandler
    : IRequestHandler<UpdateMobileAppLinkCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public UpdateMobileAppLinkCommandHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        UpdateMobileAppLinkCommand request,
        CancellationToken cancellationToken)
    {
        var application = await _db.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (application is null)
            throw new KeyNotFoundException(
                "Application not found.");

        var entity = await _db.ApplicationMobileAppLinks
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == request.ApplicationId,
                cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException(
                "Mobile app link not found.");

        entity.AndroidPackageName =
            request.AndroidPackageName?.Trim();

        entity.AndroidSha256CertificateFingerprint =
            request.AndroidSha256CertificateFingerprint?.Trim();

        entity.IosBundleId =
            request.IosBundleId?.Trim();

        entity.IosTeamId =
            request.IosTeamId?.Trim();

        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}