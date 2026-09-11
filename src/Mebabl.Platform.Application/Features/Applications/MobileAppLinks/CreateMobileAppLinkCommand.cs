using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

public sealed record CreateMobileAppLinkCommand(
    Guid ApplicationId,
    string? AndroidPackageName,
    string? AndroidSha256CertificateFingerprint,
    string? IosBundleId,
    string? IosTeamId
) : IRequest<Guid>;

public sealed class CreateMobileAppLinkCommandValidator
    : AbstractValidator<CreateMobileAppLinkCommand>
{
    public CreateMobileAppLinkCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
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

public sealed class CreateMobileAppLinkCommandHandler
    : IRequestHandler<CreateMobileAppLinkCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public CreateMobileAppLinkCommandHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<Guid> Handle(
        CreateMobileAppLinkCommand request,
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

        var entity = new ApplicationMobileAppLink
        {
            Id = Guid.NewGuid(),
            ApplicationId = request.ApplicationId,
            AndroidPackageName =
                request.AndroidPackageName?.Trim(),
            AndroidSha256CertificateFingerprint =
                request.AndroidSha256CertificateFingerprint?.Trim(),
            IosBundleId =
                request.IosBundleId?.Trim(),
            IosTeamId =
                request.IosTeamId?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ApplicationMobileAppLinks.Add(entity);

        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}