namespace Caisk.Objects;

public abstract class ObjectProfile
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? ParentName { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    
    /// <summary>
    /// Verifies that the provided property value is not null
    /// </summary>
    /// <param name="value">Property value</param>
    /// <param name="propertyName">Name of the property</param>
    /// <typeparam name="T">Property value type</typeparam>
    /// <exception cref="ProfilePropertyRequiredException">Thrown if property value is null</exception>
    protected static void PropertyRequired<T>([System.Diagnostics.CodeAnalysis.NotNull] T value, [CallerArgumentExpression("value")] string? propertyName = null)
    {
        if (value == null)
            throw new ProfilePropertyRequiredException(propertyName!);
    }

    public static bool IsNameValid(string? name) => ObjectProfileUtility.IsNameValid(name);
    protected static void ValidateName(string? name) => ObjectProfileUtility.ValidateName(name);

}