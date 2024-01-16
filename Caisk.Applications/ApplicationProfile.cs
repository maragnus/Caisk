namespace Caisk.Applications;

public class ApplicationProfile : ObjectProfile
{
    public List<ApplicationEnvironment> Environments { get; } = new();
    public string? SecureShellName { get; set; }
    public string? GitHubRepositoryName { get; set; }
    public string? MongoDatabaseName { get; set; }
}

public class ApplicationEnvironment
{
    public required string EnvironmentName { get; init; } = default!;

}