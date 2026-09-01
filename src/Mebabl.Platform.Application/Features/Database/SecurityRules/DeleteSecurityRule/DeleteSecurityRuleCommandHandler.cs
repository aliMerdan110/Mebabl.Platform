using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.DeleteSecurityRule;

public sealed class DeleteSecurityRuleCommandHandler
    : IRequestHandler<DeleteSecurityRuleCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public DeleteSecurityRuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }


    public async Task Handle(
        DeleteSecurityRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();


        var rule = await _dbContext.SecurityRules
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.Collection.ApplicationId ==
                    _currentApplication.ApplicationId,
                cancellationToken);


        if (rule is null)
            throw new Exception("Security rule not found.");


        rule.IsActive = false;


        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}