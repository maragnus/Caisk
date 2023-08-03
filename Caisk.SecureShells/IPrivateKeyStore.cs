namespace Caisk.SecureShells;

public interface IPrivateKeyStore : IObjectProfileStore<PrivateKeyProfile>
{
    Task Generate(string name, PrivateKeyType type, int? bits);
    Task Import(string name, string pemPath);
    
    Task<string[]> GetSecureShellsUsing(string name);
}