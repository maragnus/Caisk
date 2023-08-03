namespace Caisk.Objects;

internal static partial class ObjectProfileUtility
{
    [PublicAPI]
    public static bool IsNameValid(string name) =>
        NameValidationRegex().IsMatch(name);

    [System.Text.RegularExpressions.GeneratedRegex("^[A-Za-z][\\w\\-_]+[\\w]$")]
    private static partial System.Text.RegularExpressions.Regex NameValidationRegex();

    public static void ValidateName(string name)
    {
        if (IsNameValid(name)) return;
        throw new ProfileStoreException(
            "Profile name must contain at least three characters, start with an alpha, end with an alphanumeric, and contain only alphanumerics, dashes (-), and underscores (_)");
    }
}