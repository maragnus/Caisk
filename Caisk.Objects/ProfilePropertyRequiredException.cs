namespace Caisk.Objects;

public class ProfilePropertyRequiredException : Exception
{
    public ProfilePropertyRequiredException(string propertyName) 
        : base($"{propertyName} is required")
    {
    }
}