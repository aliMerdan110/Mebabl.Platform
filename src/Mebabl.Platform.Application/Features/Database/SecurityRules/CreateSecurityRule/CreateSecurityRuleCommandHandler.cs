using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;

public sealed class CreateSecurityRuleCommandHandler
    : IRequestHandler<CreateSecurityRuleCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public CreateSecurityRuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }


    public async Task<Guid> Handle(
        CreateSecurityRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();


        var rule = new SecurityRule
        {
            Id = Guid.NewGuid(),
            CollectionId = request.CollectionId,
            Permission = request.Permission.Trim(),

            CanRead = request.CanRead,
            CanWrite = request.CanWrite,
            CanDelete = request.CanDelete,
            CanQuery = request.CanQuery,

            IsActive = true
        };


        _dbContext.SecurityRules.Add(rule);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return rule.Id;
    }
}