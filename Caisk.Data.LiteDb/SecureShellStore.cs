using Caisk.SecureShells;

namespace Caisk.Data.LiteDb;

internal class SecureShellStore(ILiteCollection<SecureShellProfile> collection)
    : BaseStore<SecureShellProfile>(collection), ISecureShellStore
{
    public SecureShellProfile[] GetUsingPrivateKey(string privateKeyName) => 
        collection.Find(profile => profile.KeyPairNames.Contains(privateKeyName)).ToArray();
}