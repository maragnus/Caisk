using Caisk.Applications;
using Caisk.Docker;
using Caisk.Managers.Mongo;
using Caisk.SecureShells;

namespace Caisk.Data;

public interface IDataContext
{
    IApplicationStore ApplicationStore { get; }
    IMongoServerStore MongoServerStore { get; }
    ISecureShellStore SecureShellStore { get; }
    IPrivateKeyStore PrivateKeyStore { get; }
    IRegistryStore RegistryStore { get; }
}