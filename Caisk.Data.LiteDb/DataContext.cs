using Caisk.Applications;
using Caisk.Docker;
using Caisk.GitHub;
using Caisk.Managers.Mongo;
using Caisk.SecureShells;
using Caisk.Users;

namespace Caisk.Data.LiteDb;

public class DataContext : IDataContext
{
    public DataContext(string connectionString)
    {
        var database = new LiteDatabase(connectionString);
        ApplicationEnvironmentStore =
            new ApplicationEnvironmentStore(
                database.GetCollection<ApplicationEnvironmentProfile>(nameof(ApplicationEnvironmentStore)));
        ApplicationStore = new ApplicationStore(database.GetCollection<ApplicationProfile>(nameof(ApplicationStore)),
            ApplicationEnvironmentStore);
        MongoDatabaseStore = new MongoDatabaseStore(database.GetCollection<MongoDatabaseProfile>("MongoServerStore"));
        SecureShellStore = new SecureShellStore(database.GetCollection<SecureShellProfile>(nameof(SecureShellStore)));
        PrivateKeyStore = new PrivateKeyStore(database.GetCollection<PrivateKeyProfile>(nameof(PrivateKeyStore)));
        RegistryStore = new RegistryStore(database.GetCollection<RegistryProfile>(nameof(RegistryStore)));
        GitHubRepositoryStore =
            new GitHubRepositoryStore(database.GetCollection<GitHubRepositoryProfile>(nameof(GitHubRepositoryStore)));
        UserStore = new UserStore(database.GetCollection<UserProfile>(nameof(UserStore)));
    }

    public IApplicationStore ApplicationStore { get; }
    public IApplicationEnvironmentStore ApplicationEnvironmentStore { get; }
    public IMongoDatabaseStore MongoDatabaseStore { get; }
    public ISecureShellStore SecureShellStore { get; }
    public IPrivateKeyStore PrivateKeyStore { get; }
    public IRegistryStore RegistryStore { get; }
    public IGitHubRepositoryStore GitHubRepositoryStore { get; }
    public IUserStore UserStore { get; }
}

internal class GitHubRepositoryStore(ILiteCollection<GitHubRepositoryProfile> collection)
    : BaseStore<GitHubRepositoryProfile>(collection), IGitHubRepositoryStore
{
    public override string Name => "GitHub Repository";
}