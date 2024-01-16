using Caisk.SecureShells;

namespace Caisk.Data.LiteDb;

internal class SecureShellStore(ILiteCollection<SecureShellProfile> collection)
    : BaseStore<SecureShellProfile>(collection), ISecureShellStore
{
    public override string Name => "Secure Shell";

    public Task<SecureShellProfile[]> GetUsingPrivateKey(string privateKeyName) => 
        Task.FromResult(Collection
            .Find(profile => profile.KeyPairName == privateKeyName)
            .ToArray());
}