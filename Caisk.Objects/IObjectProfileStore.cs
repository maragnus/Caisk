namespace Caisk.Objects;

public interface IObjectProfileStore<TProfile> where TProfile : ObjectProfile, new()
{
    string Name { get; }
    ValueTask<TProfile> Create(string name, string? parentName = default);
    Task Store(TProfile profile);
    Task Rename(string oldName, string newName, string? parentName = default);
    Task Delete(string name, string? parentName = default);
    ValueTask<TProfile?> Get(string? name, string? parentName = default);
    ValueTask<TProfile[]> Get(params string[] name);
    ValueTask<TProfile[]> GetAll(string? parentName = default);
    ValueTask<string[]> GetNames(string? parentName = default);
    Task ParentRename(string oldParentName, string newParentName);
}

public class ProfileStoreException : Exception
{
    public ProfileStoreException(string message) : base(message)
    {
    }
    
    public ProfileStoreException(string message, Exception innerException) : base(message, innerException)
    {
    }
}