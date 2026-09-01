using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkAuth.GetUsers;

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItem>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetUsersQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<UserListItem>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(x => x.ApplicationId == _currentApplication.ApplicationId)
            .OrderBy(x => x.Account.Username)
            .Select(x => new UserListItem(
                x.Id,
                x.Account.Email,
                x.Account.Username,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}