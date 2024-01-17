using Caisk.Common;

namespace Caisk.Applications;

public class ApplicationEnvironmentProfile : ObjectProfile
{
    public string? MongoDatabaseName { get; set; }
    public string? SecureShellName { get; set; }
    public string? EntryUrl { get; set; }
    public string? HealthCheckUrl { get; set; }
    public string? DockerCompose { get; set; }
    public List<Service> Services { get; init; } = [];
}

public class Service
{
    public string? Name { get; set; }
    public DockerImage? Image { get; set; }
    public string? RepositoryName { get; set; }
    public string? ActionId { get; set; }
}