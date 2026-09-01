using System.Text.Json;
using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Realtime;

public sealed class RealtimeEvent : AuditableEntity
{
    public Guid ChannelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public JsonDocument Payload { get; set; } = default!;


    public Channel Channel { get; set; } = default!;
}