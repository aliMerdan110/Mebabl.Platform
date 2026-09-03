// Domain/Live/LiveStreamSession.cs

using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Domain.Live;

public class LiveStreamSession
{
    public Guid Id { get; set; }

    public Guid LiveStreamId { get; set; }

    // المستخدم الذي حصل على صلاحية النشر وبدأ هذه الجلسة.
    public Guid PublisherUserId { get; set; }

    // يتم تخزين Hash فقط.
    public string PublishTokenHash { get; set; } = string.Empty;

    // وقت انتهاء صلاحية Publish Token.
    public DateTime PublishTokenExpiresAt { get; set; }

    public LiveSessionStatus Status { get; set; }

    // وقت إنشاء Session.
    public DateTime CreatedAt { get; set; }

    // وقت انتقال Session إلى Live فعلياً.
    public DateTime? StartedAt { get; set; }

    // وقت إنهاء Session.
    public DateTime? EndedAt { get; set; }

    public LiveStream LiveStream { get; set; } = null!;
}