using Caisk.Objects;

namespace Caisk.Data.LiteDb;

internal abstract class BaseStore<TProfile>(ILiteCollection<TProfile> collection)
    : IObjectProfileStore<TProfile>
    where TProfile : ObjectProfile, new()
{
    protected ILiteCollection<TProfile> Collection { get; } = collection;

    public ValueTask<TProfile> Create(string name) => ValueTask.FromResult(new TProfile()
        {
            Name = name, 
            Id = ObjectId.NewObjectId().ToString(),
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        });

    public Task Store(TProfile profile)
    {
        profile.Updated = DateTime.Now;
        Collection.Upsert(profile.Id, profile);
        return Task.CompletedTask;
    }

    public async Task Rename(string oldName, string newName)
    {
        if (await Get(newName) is not null)
            throw new DataContextNameConflictException<TProfile>(oldName, newName);
        
        var profile = await Get(oldName)
            ?? throw new DataContextNotFoundException<TProfile>(oldName);

        profile.GetType().GetProperty(nameof(ObjectProfile.Name))!.GetSetMethod(true)!.Invoke(profile, [newName]);
        profile.Updated = DateTime.Now;
        Collection.Update(profile.Id, profile);
    }

    public async Task Delete(string name)
    {
        var profile = await Get(name)
                   ?? throw new DataContextNotFoundException<TProfile>(name);

        Collection.Delete(profile.Id);
    }

    public ValueTask<TProfile?> Get(string name) =>
        ValueTask.FromResult(Collection.FindOne(p => p.Name == name))!;

    public ValueTask<TProfile[]> Get(params string[] name) => ValueTask.FromResult(Collection.Find(p => name.Contains(p.Name)).ToArray());

    public ValueTask<TProfile[]> Get() => ValueTask.FromResult(Collection.FindAll().ToArray());
}