using Caisk.SecureShells;

namespace Caisk.Data.LiteDb;

internal class PrivateKeyStore(ILiteCollection<PrivateKeyProfile> collection)
    : BaseStore<PrivateKeyProfile>(collection), IPrivateKeyStore
{
    public override string Name => "Private Key";

    public async Task<PrivateKeyProfile> Generate(string name, PrivateKeyType type, int? bits)
    {
        var profile = PrivateKeyProfile.Generate(name, type, bits);
        await Store(profile);
        return profile;
    }

    public Task<PrivateKeyProfile> Import(string name, string pemPath)
    {
        throw new NotImplementedException();
    }
}