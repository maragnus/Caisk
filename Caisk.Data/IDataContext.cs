using Caisk.Applications;
using Caisk.Docker;
using Caisk.GitHub;
using Caisk.Managers.Mongo;
using Caisk.SecureShells;
using Caisk.Users;

namespace Caisk.Data;

public interface IDataContext
{
    IApplicationStore ApplicationStore { get; }
    IApplicationEnvironmentStore ApplicationEnvironmentStore { get; }
    IMongoDatabaseStore MongoDatabaseStore { get; }
    ISecureShellStore SecureShellStore { get; }
    IPrivateKeyStore PrivateKeyStore { get; }
    IRegistryStore RegistryStore { get; }
    IGitHubRepositoryStore GitHubRepositoryStore { get; }
    IUserStore UserStore { get; }
}