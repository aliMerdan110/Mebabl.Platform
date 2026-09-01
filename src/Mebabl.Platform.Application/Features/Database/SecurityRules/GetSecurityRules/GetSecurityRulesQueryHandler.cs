using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.GetSecurityRules;

public sealed class GetSecurityRulesQueryHandler
    : IRequestHandler<GetSecurityRulesQuery, IReadOnlyList<SecurityRuleItem>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSecurityRulesQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<IReadOnlyList<SecurityRuleItem>> Handle(
        GetSecurityRulesQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.SecurityRules
            .Where(x =>
                x.CollectionId == request.CollectionId &&
                x.IsActive)
            .Select(x => new SecurityRuleItem(
                x.Id,
                x.Permission,
                x.CanRead,
                x.CanWrite,
                x.CanDelete,
                x.CanQuery))
            .ToListAsync(cancellationToken);
    }
}