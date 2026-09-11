public class ApplicationMobileAppLink
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public string? AndroidPackageName { get; set; }

    public string? AndroidSha256CertificateFingerprint { get; set; }

    public string? IosBundleId { get; set; }

    public string? IosTeamId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}