using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class Permission : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public PlatformApplication Application { get; set; } = default!;

    /// <summary>
    /// Display name.
    /// Example: Create User
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique permission code داخل التطبيق.
    /// Example: users.create
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional grouping.
    /// Examples:
    /// Users
    /// Roles
    /// Database
    /// Storage
    /// </summary>
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; }
        = new List<RolePermission>();
}