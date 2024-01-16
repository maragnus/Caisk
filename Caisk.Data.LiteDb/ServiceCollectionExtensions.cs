using Caisk.Applications;
using Caisk.Docker;
using Caisk.GitHub;
using Caisk.Managers.Mongo;
using Caisk.Objects;
using Caisk.SecureShells;
using Microsoft.Extensions.DependencyInjection;

namespace Caisk.Data.LiteDb;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, string dbPath)
    {
        services.AddSingleton<IDataContext>(_ => new DataContext(dbPath));
        
        services.AddSingleton<IApplicationStore>(sp => sp.GetRequiredService<IDataContext>().ApplicationStore);
        services.AddSingleton<IApplicationEnvironmentStore>(sp => sp.GetRequiredService<IDataContext>().ApplicationEnvironmentStore);
        services.AddSingleton<IRegistryStore>(sp => sp.GetRequiredService<IDataContext>().RegistryStore);
        services.AddSingleton<ISecureShellStore>(sp => sp.GetRequiredService<IDataContext>().SecureShellStore);
        services.AddSingleton<IPrivateKeyStore>(sp => sp.GetRequiredService<IDataContext>().PrivateKeyStore);
        services.AddSingleton<IMongoDatabaseStore>(sp => sp.GetRequiredService<IDataContext>().MongoDatabaseStore);
        services.AddSingleton<IGitHubRepositoryStore>(sp => sp.GetRequiredService<IDataContext>().GitHubRepositoryStore);
        
        services.AddSingleton<IObjectProfileStore<ApplicationProfile>>(sp => sp.GetRequiredService<IDataContext>().ApplicationStore);
        services.AddSingleton<IObjectProfileStore<ApplicationEnvironmentProfile>>(sp => sp.GetRequiredService<IDataContext>().ApplicationEnvironmentStore);
        services.AddSingleton<IObjectProfileStore<RegistryProfile>>(sp => sp.GetRequiredService<IDataContext>().RegistryStore);
        services.AddSingleton<IObjectProfileStore<SecureShellProfile>>(sp => sp.GetRequiredService<IDataContext>().SecureShellStore);
        services.AddSingleton<IObjectProfileStore<PrivateKeyProfile>>(sp => sp.GetRequiredService<IDataContext>().PrivateKeyStore);
        services.AddSingleton<IObjectProfileStore<MongoDatabaseProfile>>(sp => sp.GetRequiredService<IDataContext>().MongoDatabaseStore);
        services.AddSingleton<IObjectProfileStore<GitHubRepositoryProfile>>(sp => sp.GetRequiredService<IDataContext>().GitHubRepositoryStore);
        return services;
    }
}