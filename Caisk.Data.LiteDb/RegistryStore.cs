using Caisk.Docker;

namespace Caisk.Data.LiteDb;

internal class RegistryStore(ILiteCollection<RegistryProfile> collection)
    : BaseStore<RegistryProfile>(collection), IRegistryStore
{
    public override string Name => "Container Registry";
}