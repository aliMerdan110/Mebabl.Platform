using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Users.GetUserById;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetUserByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<GetUserByIdResponse> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var user = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Include(x => x.Account)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (user is null)
            throw new Exception("User not found.");

        return new GetUserByIdResponse(
            user.Id,
            user.Account.Email,
            user.Account.Username,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt);
    }
}