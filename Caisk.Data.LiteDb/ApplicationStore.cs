using Caisk.Applications;

namespace Caisk.Data.LiteDb;

internal class ApplicationStore(ILiteCollection<ApplicationProfile> collection)
    : BaseStore<ApplicationProfile>(collection), IApplicationStore;