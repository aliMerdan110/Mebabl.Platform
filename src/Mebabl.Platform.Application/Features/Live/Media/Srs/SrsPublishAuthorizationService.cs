// Application/Features/Live/Media/Srs/SrsPublishAuthorizationService.cs

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Live.Sessions.ValidatePublishToken;
using Mebabl.Platform.Domain.Live.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mebabl.Platform.Application.Features.Live.Media.Srs;

public sealed class SrsPublishAuthorizationService
    : ISrsPublishAuthorizationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public SrsPublishAuthorizationService(
        IApplicationDbContext dbContext,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<bool> AuthorizePublishAsync(
        SrsPublishRequest request,
        CancellationToken cancellationToken = default)
    {
        // ---------------------------------------------------------
        // SRS sends:
        //
        // stream = sessionId
        // param  = ?token=...
        // ---------------------------------------------------------

        if (!Guid.TryParse(request.Stream, out var sessionId))
            return false;

        var token = ExtractToken(request.Param);

        if (string.IsNullOrWhiteSpace(token))
            return false;

        // ---------------------------------------------------------
        // Validate temporary Publish Token.
        // ---------------------------------------------------------

        var valid = await _mediator.Send(
            new ValidatePublishTokenCommand(
                sessionId,
                token),
            cancellationToken);

        if (!valid)
            return false;

        // ---------------------------------------------------------
        // Session must belong to a real LiveStream.
        // ---------------------------------------------------------

        var session = await _dbContext.LiveStreamSessions
            .Include(x => x.LiveStream)
            .FirstOrDefaultAsync(
                x => x.Id == sessionId,
                cancellationToken);

        if (session is null)
            return false;

        if (session.Status == LiveSessionStatus.Ended)
            return false;

        // ---------------------------------------------------------
        // SRS connection is now authorized.
        // ---------------------------------------------------------

        return true;
    }

    public async Task HandleUnpublishAsync(
        SrsPublishRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.Stream, out var sessionId))
            return;

        var session = await _dbContext.LiveStreamSessions
            .Include(x => x.LiveStream)
            .FirstOrDefaultAsync(
                x => x.Id == sessionId,
                cancellationToken);

        if (session is null)
            return;

        if (session.Status == LiveSessionStatus.Ended)
            return;

        // ---------------------------------------------------------
        // SRS confirms that publishing ended.
        // ---------------------------------------------------------

        session.Status = LiveSessionStatus.Ended;
        session.EndedAt = DateTime.UtcNow;

        session.LiveStream.Status = LiveStreamStatus.Offline;
        session.LiveStream.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static string? ExtractToken(string? param)
    {
        if (string.IsNullOrWhiteSpace(param))
            return null;

        var value = param.TrimStart('?');

        foreach (var item in value.Split('&'))
        {
            var parts = item.Split(
                '=',
                2,
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            if (!parts[0].Equals(
                    "token",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return Uri.UnescapeDataString(parts[1]);
        }

        return null;
    }
}