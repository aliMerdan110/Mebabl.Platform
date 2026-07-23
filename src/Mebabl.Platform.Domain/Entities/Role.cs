namespace Mebabl.Platform.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}