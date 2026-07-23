namespace Mebabl.Platform.Domain.Entities;

public class ApplicationUser
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public Guid AccountId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
}