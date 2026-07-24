namespace Mebabl.Platform.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // أضف هذا السطر لتكتمل العلاقة
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}