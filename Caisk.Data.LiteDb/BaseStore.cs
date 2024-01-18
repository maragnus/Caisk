using Caisk.Objects;

namespace Caisk.Data.LiteDb;

internal abstract class BaseStore<TProfile>(ILiteCollection<TProfile> collection)
    : IObjectProfileStore<TProfile>
    where TProfile : ObjectProfile, new()
{
    protected ILiteCollection<TProfile> Collection { get; } = collection;

    public virtual string Name { get; } = typeof(TProfile).Name;

    public ValueTask<TProfile> Create(string name, string? parentName = default) => ValueTask.FromResult(new TProfile()
    {
        Name = name,
        ParentName = parentName,
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

    public virtual async Task Rename(string oldName, string newName, string? parentName = default)
    {
        if (await Get(newName, parentName) is not null)
            throw new DataContextNameConflictException<TProfile>(oldName, newName);

        var profile = await Get(oldName, parentName)
                      ?? throw new DataContextNotFoundException<TProfile>(oldName);

        profile.GetType().GetProperty(nameof(ObjectProfile.Name))!.GetSetMethod(true)!.Invoke(profile, [newName]);
        profile.Updated = DateTime.Now;
        Collection.Update(profile.Id, profile);
    }

    public async Task Delete(string name, string? parentName = default)
    {
        var profile = await Get(name, parentName)
                      ?? throw new DataContextNotFoundException<TProfile>(name);

        Collection.Delete(profile.Id);
    }

    public ValueTask<TProfile?> TryGet(string? name, string? parentName = default) =>
        ValueTask.FromResult(name == null
            ? default
            : Collection.FindOne(p => p.Name == name && p.ParentName == parentName));

    public ValueTask<TProfile?> Get(string? name, string? parentName = default) =>
        ValueTask.FromResult(name == null
            ? default
            : Collection.FindOne(p => p.Name == name && p.ParentName == parentName) ??
              throw new ProfileNotFoundException<TProfile>(name, parentName));

    public ValueTask<TProfile> Require(string name, string? parentName = default) =>
        ValueTask.FromResult(Collection.FindOne(p => p.Name == name && p.ParentName == parentName) ??
                             throw new ProfileNotFoundException<TProfile>(name, parentName));

    public ValueTask<TProfile[]> Get(params string[] name) =>
        ValueTask.FromResult(Collection.Find(p => name.Contains(p.Name)).ToArray());

    public ValueTask<TProfile[]> GetAll(string? parentName = default) =>
        ValueTask.FromResult(Collection.Find(p => p.ParentName == parentName).ToArray());

    public ValueTask<string[]> GetNames(string? parentName = default) =>
        ValueTask.FromResult(Collection.Find(p => p.ParentName == parentName).Select(p => p.Name).ToArray());

    public Task ParentRename(string oldParentName, string newParentName)
    {
        foreach (var profile in Collection.Find(p => p.ParentName == oldParentName))
        {
            profile.ParentName = newParentName;
            Collection.Update(profile);
        }

        return Task.CompletedTask;
    }
}