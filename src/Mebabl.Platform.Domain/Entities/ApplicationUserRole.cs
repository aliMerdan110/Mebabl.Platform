namespace Mebabl.Platform.Domain.Entities;

public class ApplicationUserRole
{
    public Guid Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime CreatedAt { get; set; }
}