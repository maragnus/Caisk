namespace Caisk.Applications;

public class ApplicationProfile : ObjectProfile
{
    public List<ApplicationEnvironment> Environments { get; } = new();
}

public class ApplicationEnvironment
{
    public required string EnvironmentName { get; init; } = default!;

}