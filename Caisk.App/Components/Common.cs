using Caisk.Applications;
using Caisk.Docker;
using Caisk.GitHub;
using Caisk.Managers.Mongo;
using Caisk.Objects;
using Caisk.SecureShells;
using Microsoft.AspNetCore.Components;

namespace Caisk.App.Components;

public static class Common
{
    public const string SecureShellProfileUrl = "/shells";
    public const string MongoDatabaseProfileUrl = "/mongo";
    public const string PrivateKeyProfileUrl = "/keys";
    public const string ApplicationProfileUrl = "/apps";
    public const string ApplicationEnvironmentProfileUrl = "/apps/{parent}";
    public const string GitHubRepositoryProfileUrl = "/github";
    public const string RegistryProfileUrl = "/registries";

    private static Dictionary<Type, string> _profileMap = new()
    {
        { typeof(SecureShellProfile), SecureShellProfileUrl },
        { typeof(MongoDatabaseProfile), MongoDatabaseProfileUrl },
        { typeof(PrivateKeyProfile), PrivateKeyProfileUrl },
        { typeof(ApplicationProfile), ApplicationProfileUrl },
        { typeof(ApplicationEnvironmentProfile), ApplicationEnvironmentProfileUrl },
        { typeof(GitHubRepositoryProfile), GitHubRepositoryProfileUrl },
        { typeof(RegistryProfile), RegistryProfileUrl }
    };

    public static string ProfilesUrl<TProfile>(string? parentName = default)
        where TProfile : ObjectProfile, new() =>
        parentName == null
            ? _profileMap.GetValueOrDefault(typeof(TProfile)) ??
              throw new Exception($"{typeof(TProfile)} not registered in Common.ProfileMap")
            : ProfilesUrl<TProfile>(default).Replace("{parent}", parentName);

    public static string ProfileUrl<TProfile>(string name, string? parentName = default)
        where TProfile : ObjectProfile, new() => $"{ProfilesUrl<TProfile>(parentName)}/{name}";

    public static void NavigateToProfile<TProfile>(this NavigationManager navigationManager, string name,
        string? parentName = default) where TProfile : ObjectProfile, new() =>
        navigationManager.NavigateTo(ProfileUrl<TProfile>(name, parentName));

    public static void NavigateToProfileList<TProfile>(this NavigationManager navigationManager,
        string? parentName = default) where TProfile : ObjectProfile, new() =>
        navigationManager.NavigateTo(ProfilesUrl<TProfile>(parentName));

    public static RenderFragment Date(DateTime dateTime) => builder =>
    {
        var localTime = dateTime.ToLocalTime();
        builder.OpenElement(0, "time");
        builder.AddAttribute(1, "datetime", localTime.ToString("G"));
        builder.AddAttribute(1, "title", localTime.ToString("F"));
        builder.AddContent(2, When(DateOnly.FromDateTime(localTime)));
        builder.CloseElement();
    };

    private static string When(DateOnly date)
    {
        var now = DateOnly.FromDateTime(DateTime.Today);
        var diff = date.DayNumber - now.DayNumber;
        var unit = "days";

        if (Math.Abs(diff) >= 14)
        {
            diff /= 7;
            unit = "weeks";
        }

        return diff switch
        {
            0 => "today",
            < 0 => $"{-diff} {unit} ago",
            > 0 => $"in {diff} {unit}"
        };
    }
}