namespace Mebabl.Platform.Domain.Live;

public class StreamCredential
{
    public Guid Id { get; set; }

    public Guid LiveStreamId { get; set; }

    public string KeyHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public LiveStream LiveStream { get; set; } = null!;
}