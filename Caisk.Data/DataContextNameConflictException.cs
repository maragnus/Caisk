namespace Caisk.Data;

public class DataContextNameConflictException<TProfile> : DataContextNameConflictException
{
    public DataContextNameConflictException(string oldName, string newName) 
        : base($@"{typeof(TProfile).Name} ""{oldName}"" cannot be renamed to ""{newName}"" because a profile with that name already exists")
    {
    }    
}
public class DataContextNameConflictException : Exception
{
    public DataContextNameConflictException(string message, Exception innerException) : base(message, innerException) {}
    public DataContextNameConflictException(string message) : base(message) {}
}

public class DataContextNotFoundException<TProfile> : DataContextNotFoundException
{   
    public DataContextNotFoundException(string name) 
        : base($@"{typeof(TProfile).Name} ""{name}"" does not exist")
    {
    }    
}

public class DataContextNotFoundException : Exception
{
    public DataContextNotFoundException(string message, Exception innerException) : base(message, innerException) {}
    public DataContextNotFoundException(string message) : base(message) {}
}