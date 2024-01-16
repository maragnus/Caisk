namespace Caisk.SecureShells;

public interface ISecureShellStore : IObjectProfileStore<SecureShellProfile>
{
    Task<SecureShellProfile[]> GetUsingPrivateKey(string privateKeyName);
}