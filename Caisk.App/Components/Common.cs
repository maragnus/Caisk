using Caisk.Applications;
using Caisk.Docker;
using Caisk.GitHub;
using Caisk.Managers.Mongo;
using Caisk.Objects;
using Caisk.SecureShells;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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

    public const string DashboardIcon = Icons.Material.Filled.Dashboard;
    public const string SecureShellProfileIcon = Icons.Material.Filled.Terminal;
    public const string MongoDatabaseProfileIcon = Icons.Material.Filled.BackupTable;
    public const string PrivateKeyProfileIcon = Icons.Material.Filled.Key;
    public const string ApplicationProfileIcon = Icons.Material.Filled.Apps;
    public const string ApplicationEnvironmentProfileIcon = Icons.Material.Filled.LocalShipping;
    public const string GitHubRepositoryProfileIcon = Icons.Custom.Brands.GitHub;
    public const string RegistryProfileIcon = Icons.Material.Filled.DirectionsBoat;

    public const string DockerComposeYaml = "docker-compose.yaml";

    private static readonly Dictionary<Type, string> IconMap = new()
    {
        { typeof(SecureShellProfile), SecureShellProfileIcon },
        { typeof(MongoDatabaseProfile), MongoDatabaseProfileIcon },
        { typeof(PrivateKeyProfile), PrivateKeyProfileIcon },
        { typeof(ApplicationProfile), ApplicationProfileIcon },
        { typeof(ApplicationEnvironmentProfile), ApplicationEnvironmentProfileIcon },
        { typeof(GitHubRepositoryProfile), GitHubRepositoryProfileIcon },
        { typeof(RegistryProfile), RegistryProfileIcon }
    };

    private static readonly Dictionary<Type, string> ProfileMap = new()
    {
        { typeof(SecureShellProfile), SecureShellProfileUrl },
        { typeof(MongoDatabaseProfile), MongoDatabaseProfileUrl },
        { typeof(PrivateKeyProfile), PrivateKeyProfileUrl },
        { typeof(ApplicationProfile), ApplicationProfileUrl },
        { typeof(ApplicationEnvironmentProfile), ApplicationEnvironmentProfileUrl },
        { typeof(GitHubRepositoryProfile), GitHubRepositoryProfileUrl },
        { typeof(RegistryProfile), RegistryProfileUrl }
    };

    public static string ProfileIcon<TProfile>()
        where TProfile : ObjectProfile, new() =>
        IconMap.GetValueOrDefault(typeof(TProfile), Icons.Material.Filled.QuestionMark);

    public static string ProfilesUrl<TProfile>(string? parentName = default)
        where TProfile : ObjectProfile, new() =>
        parentName == null
            ? ProfileMap.GetValueOrDefault(typeof(TProfile)) ??
              throw new Exception($"{typeof(TProfile)} not registered in Common.ProfileMap")
            : ProfilesUrl<TProfile>().Replace("{parent}", parentName);

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