using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.React;

public sealed class ReactCommandHandler
    : IRequestHandler<ReactCommand, ReactionDto>
{
    private const int MaximumReactionTypeLength = 32;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReactCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ReactionDto> Handle(
        ReactCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        if (applicationId == Guid.Empty ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        if (request.PostId == Guid.Empty)
        {
            throw new ArgumentException(
                "Post ID is required.",
                nameof(request.PostId));
        }

        var type = string.IsNullOrWhiteSpace(request.Type)
            ? "Like"
            : request.Type.Trim();

        if (type.Length > MaximumReactionTypeLength ||
            type.Any(char.IsControl))
        {
            throw new ArgumentException(
                "The reaction type is invalid.");
        }

        var postExists = await _db.Posts
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.PostId &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!postExists)
        {
            throw new KeyNotFoundException("Post not found.");
        }

        var reaction = await _db.SocialReactions
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.PostId == request.PostId &&
                    x.UserId == userId,
                cancellationToken);

        var now = DateTime.UtcNow;

        if (reaction is not null)
        {
            reaction.Type = type;
            reaction.UpdatedAt = now;

            await _db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            reaction = new SocialReaction
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                PostId = request.PostId,
                UserId = userId,
                Type = type,
                CreatedAt = now
            };

            _db.SocialReactions.Add(reaction);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // إزالة الكيان المضاف محليًا بعد فشل الحفظ.
                _db.SocialReactions.Entry(reaction).State =
                    EntityState.Detached;

                // ربما سبق الطلب المتزامن الآخر وأدرج التفاعل.
                var existingReaction = await _db.SocialReactions
                    .FirstOrDefaultAsync(
                        x =>
                            x.ApplicationId == applicationId &&
                            x.PostId == request.PostId &&
                            x.UserId == userId,
                        cancellationToken);

                // إذا لم يوجد سجل منافس، فليس لدينا ما يثبت
                // أن الخطأ ناتج عن تكرار؛ أعد إطلاق الخطأ الأصلي.
                if (existingReaction is null)
                {
                    throw;
                }

                existingReaction.Type = type;
                existingReaction.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(cancellationToken);

                reaction = existingReaction;
            }
        }

        return new ReactionDto(
            reaction.Id,
            reaction.PostId,
            reaction.UserId,
            reaction.Type,
            reaction.CreatedAt);
    }
}