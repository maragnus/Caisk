using Caisk.Objects;

namespace Caisk.Data;

[Serializable]
public class ProfileNotFoundException(Type type, string profileName, string? parentName)
    : Exception($"{type.Name} with name {ToFullName(profileName, parentName)} does not exists")
{
    public string? ParentName { get; set; } = parentName;

    public string Name { get; set; } = profileName;

    public Type ProfileType { get; set; } = type;

    private static string ToFullName(string profileName, string? parentName) =>
        string.IsNullOrWhiteSpace(parentName)
            ? profileName
            : $"{parentName}/{profileName}";
}

public class ProfileNotFoundException<TProfile>(string profileName, string? parentName = default)
    : ProfileNotFoundException(typeof(TProfile), profileName, parentName)
    where TProfile : ObjectProfile, new();