using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Domain.Live;

public class LiveStream
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public LiveStreamStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<StreamCredential> Credentials { get; set; }
        = new List<StreamCredential>();

    public ICollection<LiveStreamSession> Sessions { get; set; }
        = new List<LiveStreamSession>();
}