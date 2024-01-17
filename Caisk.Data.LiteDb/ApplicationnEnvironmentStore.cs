using Caisk.Applications;

namespace Caisk.Data.LiteDb;

internal class ApplicationEnvironmentStore(ILiteCollection<ApplicationEnvironmentProfile> collection)
    : BaseStore<ApplicationEnvironmentProfile>(collection), IApplicationEnvironmentStore
{
    public override string Name => "Application Environment";
    public Task<ApplicationEnvironmentProfile[]> GetByApplication(string applicationName) =>
        Task.FromResult(Collection.Find(profile => profile.ParentName == applicationName).ToArray());
}