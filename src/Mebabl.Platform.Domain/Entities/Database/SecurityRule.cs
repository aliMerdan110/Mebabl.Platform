using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Domain.Entities.Database;

public class SecurityRule : AuditableEntity
{
    public Guid CollectionId { get; set; }

    public Collection Collection { get; set; } = default!;


    public string Permission { get; set; } = string.Empty;


    public bool CanRead { get; set; } = true;

    public bool CanWrite { get; set; } = false;

    public bool CanDelete { get; set; } = false;

    public bool CanQuery { get; set; } = false;


    public bool IsActive { get; set; } = true;
}