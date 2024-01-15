namespace Caisk.SecureShells;

public interface IPrivateKeyStore : IObjectProfileStore<PrivateKeyProfile>
{
    Task<PrivateKeyProfile> Generate(string name, PrivateKeyType type, int? bits);
    Task<PrivateKeyProfile> Import(string name, string pemPath);
}