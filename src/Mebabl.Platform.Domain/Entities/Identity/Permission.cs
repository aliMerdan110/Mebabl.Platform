using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class Permission : AuditableEntity
{

    public Guid ApplicationId { get; set; }
    public PlatformApplication  Application { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;


    // أضف هذا السطر لتكتمل العلاقة
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}