using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Realtime;

public sealed class Channel : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;


    public ICollection<RealtimeEvent> Events { get; set; }
        = new List<RealtimeEvent>();
}