using Caisk.Managers.Mongo;

namespace Caisk.Data.LiteDb;

internal class MongoDatabaseStore(ILiteCollection<MongoDatabaseProfile> collection)
    : BaseStore<MongoDatabaseProfile>(collection), IMongoDatabaseStore
{
    public override string Name => "Mongo Server";
}