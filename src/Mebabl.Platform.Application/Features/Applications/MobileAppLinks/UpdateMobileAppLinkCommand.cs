using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

public sealed record UpdateMobileAppLinkCommand(
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
    private readonly ICurrentApplication _currentApplication;

    public UpdateMobileAppLinkCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        UpdateMobileAppLinkCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        var entity = await _db.ApplicationMobileAppLinks
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.ApplicationId == applicationId,
                cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException("Mobile app link not found.");

        entity.AndroidPackageName = request.AndroidPackageName?.Trim();
        entity.AndroidSha256CertificateFingerprint =
            request.AndroidSha256CertificateFingerprint?.Trim();
        entity.IosBundleId = request.IosBundleId?.Trim();
        entity.IosTeamId = request.IosTeamId?.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}