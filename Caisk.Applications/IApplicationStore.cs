namespace Caisk.Applications;

public interface IApplicationStore : IObjectProfileStore<ApplicationProfile>
{
    Task<ApplicationEnvironments[]> GetAllEnvironments();
}

public record ApplicationEnvironments(ApplicationProfile Application, ApplicationEnvironmentProfile[] Environments);