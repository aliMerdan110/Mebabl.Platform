// Application/Features/Live/Sessions/PublishStream/PublishStreamTokenValidator.cs

using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;

public sealed class PublishStreamTokenValidator
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public PublishStreamTokenValidator(
        IApplicationDbContext dbContext,
        IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<bool> ValidateAsync(
        Guid sessionId,
        string publishToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publishToken))
            return false;

        var session = await _dbContext.LiveStreamSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == sessionId,
                cancellationToken);

        if (session is null)
            return false;

        // الجلسة المنتهية تفقد صلاحية الـ token فوراً.
        if (session.Status == LiveSessionStatus.Ended)
            return false;

        // انتهاء الوقت يلغي الـ token.
        if (_clock.UtcNow >= session.PublishTokenExpiresAt)
            return false;

        var suppliedHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(publishToken)));

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(session.PublishTokenHash),
            Convert.FromHexString(suppliedHash));
    }
}



