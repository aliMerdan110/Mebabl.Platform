// Application/Features/Live/Sessions/ValidatePublishToken/ValidatePublishTokenCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.Live;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Sessions.ValidatePublishToken;

public sealed class ValidatePublishTokenCommandHandler
    : IRequestHandler<ValidatePublishTokenCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPublishTokenService _publishTokenService;
    private readonly IClock _clock;

    public ValidatePublishTokenCommandHandler(
        IApplicationDbContext dbContext,
        IPublishTokenService publishTokenService,
        IClock clock)
    {
        _dbContext = dbContext;
        _publishTokenService = publishTokenService;
        _clock = clock;
    }

    public async Task<bool> Handle(
        ValidatePublishTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PublishToken))
            return false;

        var session = await _dbContext.LiveStreamSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.SessionId,
                cancellationToken);

        if (session is null)
            return false;

        // Session انتهت.
        if (session.Status == LiveSessionStatus.Ended)
            return false;

        // Publish Token انتهت صلاحيته.
        if (_clock.UtcNow >= session.PublishTokenExpiresAt)
            return false;

        // مقارنة Hash باستخدام Constant-Time Comparison.
        return _publishTokenService.VerifyToken(
            request.PublishToken,
            session.PublishTokenHash);
    }
}