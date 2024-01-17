using System.Text.RegularExpressions;

namespace Caisk.Docker;

public readonly struct DockerImage(string? registry, string? imageName, string? tag)
{
    public string? Registry { get; } = registry;
    public string? ImageName { get; } = imageName;
    public string? Tag { get; } = tag;
    public decimal? TagNumber { get; } = tag != null && decimal.TryParse(tag, out var result) ? result : null;
    public bool IsTagNumeric => TagNumber.HasValue;

    public static DockerImage Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new DockerImage(null, null, null);
        var parts = Regex.Match(value, @"(([\w.]+)/)?(\w+)(:(\w+))");
        return new DockerImage(parts.Groups[2].Value, parts.Groups[3].Value, parts.Groups[5].Value);
    }

    public override string ToString()
    {
        return $"{Registry}/{ImageName}:{Tag}";
    }
}