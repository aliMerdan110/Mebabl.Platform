using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

public sealed class UpdateSecurityRuleCommandHandler
    : IRequestHandler<UpdateSecurityRuleCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public UpdateSecurityRuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }


    public async Task Handle(
        UpdateSecurityRuleCommand request,
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


        rule.Permission = request.Permission.Trim();

        rule.CanRead = request.CanRead;
        rule.CanWrite = request.CanWrite;
        rule.CanDelete = request.CanDelete;
        rule.CanQuery = request.CanQuery;


        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}