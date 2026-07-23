namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Category
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public DateTime CreatedAt { get; set; }
}