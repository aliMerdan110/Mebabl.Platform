using FluentValidation;
using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

public sealed record CreateMobileAppLinkCommand(
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
    private readonly ICurrentApplication _currentApplication;

    public CreateMobileAppLinkCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<Guid> Handle(
        CreateMobileAppLinkCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        var entity = new ApplicationMobileAppLink
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            AndroidPackageName = request.AndroidPackageName?.Trim(),
            AndroidSha256CertificateFingerprint =
                request.AndroidSha256CertificateFingerprint?.Trim(),
            IosBundleId = request.IosBundleId?.Trim(),
            IosTeamId = request.IosTeamId?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ApplicationMobileAppLinks.Add(entity);

        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}