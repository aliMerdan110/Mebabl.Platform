namespace Mebabl.Platform.Domain.Entities;

public class ApplicationUserRole
{
    public Guid Id { get; set; }

    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}