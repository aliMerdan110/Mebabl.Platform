using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkAuth.GetUserById;

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
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (user is null)
            throw new Exception("User not found.");

        return new GetUserByIdResponse(
            user.Id,
            user.Email,
            user.Username,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt);
    }
}