using Caisk.Applications;

namespace Caisk.Data.LiteDb;

internal class ApplicationStore(ILiteCollection<ApplicationProfile> collection, IApplicationEnvironmentStore environmentStore)
    : BaseStore<ApplicationProfile>(collection), IApplicationStore
{
    
    public override string Name => "Application";

    public override async Task Rename(string oldName, string newName, string? parentName = default)
    {
        await base.Rename(oldName, newName, parentName);
        await environmentStore.ParentRename(oldName, newName);
    }
}