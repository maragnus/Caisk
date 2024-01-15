namespace Caisk.SecureShells;

public interface ISecureShellStore : IObjectProfileStore<SecureShellProfile>
{
    SecureShellProfile[] GetUsingPrivateKey(string privateKeyName);
}