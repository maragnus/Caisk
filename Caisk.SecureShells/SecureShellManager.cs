namespace Caisk.SecureShells;

public enum PrivateKeyType
{
    Rsa,
    Dsa,
    Ecdsa,
    Ed22519
}

[PublicAPI]
public class SecureShellManager
{
    public ISecureShellStore SecureShellStore { get; }
    public IPrivateKeyStore PrivateKeyStore { get; }

    public SecureShellManager(ISecureShellStore sshStore, IPrivateKeyStore ppkStore)
    {
        SecureShellStore = sshStore;
        PrivateKeyStore = ppkStore;
    }

    public async Task<SecureShell> CreateSecureShell(string name)
    {
        var profile = await SecureShellStore.Get(name);
        return new SecureShell(profile!, this); 
    }
}