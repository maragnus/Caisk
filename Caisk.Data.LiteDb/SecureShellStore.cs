using Caisk.SecureShells;

namespace Caisk.Data.LiteDb;

internal class SecureShellStore(ILiteCollection<SecureShellProfile> collection)
    : BaseStore<SecureShellProfile>(collection), ISecureShellStore
{
    public override string Name => "Secure Shell";

    public SecureShellProfile[] GetUsingPrivateKey(string privateKeyName) =>
        Collection.Find(profile => profile.KeyPairNames.Contains(privateKeyName)).ToArray();
}