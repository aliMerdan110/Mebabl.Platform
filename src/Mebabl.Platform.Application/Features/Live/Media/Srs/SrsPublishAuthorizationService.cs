using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Live.Sessions.ValidatePublishToken;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.Live;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Media.Srs;

public sealed class SrsPublishAuthorizationService
    : ISrsPublishAuthorizationService
{
    private const string ExpectedApp = "live";

    private readonly IApplicationDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IPublishTokenService _publishTokenService;
    private readonly IClock _clock;

    public SrsPublishAuthorizationService(
        IApplicationDbContext dbContext,
        IMediator mediator,
        IPublishTokenService publishTokenService,
        IClock clock)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _publishTokenService = publishTokenService;
        _clock = clock;
    }

    public async Task<bool> AuthorizePublishAsync(
        SrsPublishRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsExpectedRequest(request, "on_publish"))
            return false;

        var sessionIdText = ExtractParam(
            request.Param,
            "sessionId");

        var token = ExtractParam(
            request.Param,
            "token");

        if (!Guid.TryParse(sessionIdText, out var sessionId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var valid = await _mediator.Send(
            new ValidatePublishTokenCommand(sessionId, token),
            cancellationToken);

        if (!valid)
            return false;

        var session = await _dbContext.LiveStreamSessions
            .Include(x => x.LiveStream)
            .FirstOrDefaultAsync(
                x => x.Id == sessionId,
                cancellationToken);

        if (session is null || session.LiveStream is null)
            return false;

        // يجب أن يطابق اسم البث الطلب والجلسة الفعلية.
        if (!string.Equals(
                request.Stream,
                session.LiveStream.Name,
                StringComparison.Ordinal))
        {
            return false;
        }

        // إعادة استدعاء الخطاف للجلسة النشطة لا تغيّر حالتها.
        if (session.Status == LiveSessionStatus.Live)
            return true;

        if (session.Status != LiveSessionStatus.Starting)
            return false;

        var now = _clock.UtcNow;

        session.Status = LiveSessionStatus.Live;
        session.StartedAt = now;

        session.LiveStream.Status = LiveStreamStatus.Live;
        session.LiveStream.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task HandleUnpublishAsync(
        SrsPublishRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsExpectedRequest(request, "on_unpublish"))
            return;

        var sessionIdText = ExtractParam(
            request.Param,
            "sessionId");

        var token = ExtractParam(
            request.Param,
            "token");

        if (!Guid.TryParse(sessionIdText, out var sessionId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        var session = await _dbContext.LiveStreamSessions
            .Include(x => x.LiveStream)
            .FirstOrDefaultAsync(
                x => x.Id == sessionId,
                cancellationToken);

        if (session is null ||
            session.LiveStream is null ||
            session.Status == LiveSessionStatus.Ended)
        {
            return;
        }

        // لا نسمح بتغيير جلسة باستخدام اسم بث مختلف.
        if (!string.Equals(
                request.Stream,
                session.LiveStream.Name,
                StringComparison.Ordinal))
        {
            return;
        }

        // نتحقق من الرمز المخزن حتى لو انتهت صلاحيته الزمنية؛
        // فقد يستمر بث صالح لأكثر من 15 دقيقة.
        if (!_publishTokenService.VerifyToken(
                token,
                session.PublishTokenHash))
        {
            return;
        }

        var now = _clock.UtcNow;

        session.Status = LiveSessionStatus.Ended;
        session.EndedAt = now;

        // لا نوقف البث ما دامت هناك جلسة أخرى غير منتهية.
        var hasOtherActiveSession =
            await _dbContext.LiveStreamSessions
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.LiveStreamId == session.LiveStreamId &&
                        x.Id != session.Id &&
                        x.Status != LiveSessionStatus.Ended,
                    cancellationToken);

        if (!hasOtherActiveSession)
        {
            session.LiveStream.Status = LiveStreamStatus.Offline;
            session.LiveStream.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsExpectedRequest(
        SrsPublishRequest request,
        string expectedAction)
    {
        return request is not null
            && string.Equals(
                request.Action,
                expectedAction,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                request.App,
                ExpectedApp,
                StringComparison.Ordinal);
    }

    private static string? ExtractParam(
        string? param,
        string key)
    {
        if (string.IsNullOrWhiteSpace(param))
            return null;

        string? result = null;

        foreach (var item in param.TrimStart('?').Split(
                     '&',
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = item.IndexOf('=');

            if (separator <= 0)
                continue;

            var name = Uri.UnescapeDataString(
                item[..separator]);

            if (!name.Equals(
                    key,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // نرفض المعامل إذا تكرر، منعًا لالتباس تفسيره.
            if (result is not null)
                return null;

            result = Uri.UnescapeDataString(
                item[(separator + 1)..]);
        }

        return result;
    }
}