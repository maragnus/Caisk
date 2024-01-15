namespace Caisk.Objects;

public interface IObjectProfileStore<TProfile> where TProfile : ObjectProfile, new()
{
    ValueTask<TProfile> Create(string name);
    Task Store(TProfile profile);
    Task Rename(string oldName, string newName);
    Task Delete(string name);
    ValueTask<TProfile?> Get(string name);
    ValueTask<TProfile[]> Get(params string[] name);
    ValueTask<TProfile[]> Get();
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