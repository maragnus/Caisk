namespace Caisk.Applications;

public class ApplicationEnvironmentProfile : ObjectProfile
{
    public string ApplicationName { get; set; } = default!;
    public string? MongoDatabaseName { get; set; }
    public string? SecureShellName { get; set; }
    public string? EntryUrl { get; set; }
    public string? HealthCheckUrl { get; set; }
}