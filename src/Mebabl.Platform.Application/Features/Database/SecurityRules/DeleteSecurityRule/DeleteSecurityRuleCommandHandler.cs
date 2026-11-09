using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.DeleteSecurityRule;

/// <summary>
/// Deactivates and soft-deletes a collection policy owned by the authenticated
/// developer. The revision is incremented to reject stale updates.
/// </summary>
public sealed class DeleteSecurityRuleCommandHandler
    : IRequestHandler<DeleteSecurityRuleCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;
    private readonly IClock _clock;

    public DeleteSecurityRuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper,
        IClock clock)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentDeveloper = currentDeveloper
            ?? throw new ArgumentNullException(nameof(currentDeveloper));
        _clock = clock
            ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task Handle(
        DeleteSecurityRuleCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated developer is required.");
        }

        var developerId = _currentDeveloper.DeveloperId;
        if (developerId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated developer context is invalid.");
        }

        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Security policy ID is required.",
                nameof(request.Id));
        }

        if (request.ExpectedRevision < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.ExpectedRevision),
                "Expected revision must be a positive number.");
        }

        var policy = await _dbContext.SecurityRules
            .FirstOrDefaultAsync(
                rule =>
                    rule.Id == request.Id &&
                    !rule.IsDeleted &&
                    !rule.Collection.IsDeleted &&
                    !rule.Collection.Project.IsDeleted &&
                    rule.Collection.Project.DeveloperId == developerId,
                cancellationToken);

        if (policy is null)
        {
            throw new KeyNotFoundException("Security policy not found.");
        }

        if (policy.Revision != request.ExpectedRevision)
        {
            throw new InvalidOperationException(
                "Security policy revision conflict. Reload the policy before deleting it.");
        }

        if (policy.Revision == int.MaxValue)
        {
            throw new InvalidOperationException(
                "The security policy revision limit has been reached.");
        }

        policy.IsActive = false;
        policy.PublishedAt = null;
        policy.IsDeleted = true;
        policy.Revision++;
        policy.UpdatedAt = _clock.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InvalidOperationException(
                "Security policy was modified by another request. Reload it and try again.",
                exception);
        }
    }
}
