namespace Mebabl.Platform.Domain.Entities;

public class Profile
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}