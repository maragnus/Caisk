using Caisk.Objects;
using Microsoft.AspNetCore.Components;

namespace Caisk.App.Components.Pages;

public class BaseProfileEditor<TProfile, TStore> : BasePage
    where TProfile : ObjectProfile, new()
    where TStore : IObjectProfileStore<TProfile>
{
    [Inject] public TStore ProfileStore { get; set; } = default!;
    [Parameter] public string? Name { get; set; }
    protected TProfile Profile { get; set; } = default!;
    protected bool IsValid { get; set; }
    protected bool IsNew { get; set; }

    protected void Return()
    {
        var path = new UriBuilder(NavigationManager.Uri).Path;
        var parent = path[..path.IndexOf('/', 1)];
        NavigationManager.NavigateTo(parent);
    }

    protected override async Task OnSafeInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Return();
            return;
        }

        var profile = await ProfileStore.Get(Name);
        IsNew = profile == null;
        Profile = profile ?? await ProfileStore.Create(Name);
    }

    protected async Task Store()
    {
        await ProfileStore.Store(Profile);
        Return();
    }

    protected async Task Delete()
    {
        var result = await DialogService.ShowMessageBox($"Delete {ProfileStore.Name} Profile", "Are you sure that you would like to delete this profile?", "Yes", "No", "Cancel");
        if (result != true) return;
        await ProfileStore.Delete(Profile.Name);
        Return();
    }
}