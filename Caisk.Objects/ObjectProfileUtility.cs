namespace Caisk.Objects;

public static partial class ObjectProfileUtility
{
    public const string NameConstraint =
        "Profile name must contain at least three characters, start with an alpha, end with an alphanumeric, and contain only alphanumerics, dashes (-), and underscores (_)";

    public const string NameRegex = @"^[A-Za-z][\w\-_]+[\w]$";

    [PublicAPI]
    public static bool IsNameValid(string? name) => 
        !string.IsNullOrWhiteSpace(name)
        && NameValidationRegex().IsMatch(name);

    [System.Text.RegularExpressions.GeneratedRegex(NameRegex)]
    private static partial System.Text.RegularExpressions.Regex NameValidationRegex();

    public static void ValidateName(string? name)
    {
        if (IsNameValid(name)) return;
        throw new ProfileStoreException(NameConstraint);
    }
}