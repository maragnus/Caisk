using Caisk.Applications;
using Caisk.Docker;
using Caisk.GitHub;
using Caisk.Managers.Mongo;
using Caisk.SecureShells;

namespace Caisk.Data.LiteDb;

public class DataContext : IDataContext
{
    public DataContext(string connectionString)
    {
        var database = new LiteDatabase(connectionString);
        ApplicationStore = new ApplicationStore(database.GetCollection<ApplicationProfile>(nameof(ApplicationStore)));
        MongoServerStore = new MongoServerStore(database.GetCollection<MongoServerProfile>(nameof(MongoServerStore)));
        SecureShellStore = new SecureShellStore(database.GetCollection<SecureShellProfile>(nameof(SecureShellStore)));
        PrivateKeyStore = new PrivateKeyStore(database.GetCollection<PrivateKeyProfile>(nameof(PrivateKeyStore)));
        RegistryStore = new RegistryStore(database.GetCollection<RegistryProfile>(nameof(RegistryStore)));
        GitHubRepositoryStore = new GitHubRepositoryStore(database.GetCollection<GitHubRepositoryProfile>(nameof(GitHubRepositoryStore)));
    }
    
    public IApplicationStore ApplicationStore { get; }
    public IMongoServerStore MongoServerStore { get; }
    public ISecureShellStore SecureShellStore { get; }
    public IPrivateKeyStore PrivateKeyStore { get; }
    public IRegistryStore RegistryStore { get; }
    public IGitHubRepositoryStore GitHubRepositoryStore { get; }
}

internal class GitHubRepositoryStore(ILiteCollection<GitHubRepositoryProfile> collection) : BaseStore<GitHubRepositoryProfile>(collection), IGitHubRepositoryStore
{
    public override string Name => "GitHub Repository";
}