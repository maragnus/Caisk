﻿using Caisk.App.Components.Modals;
using Caisk.Objects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Caisk.App.Components.Pages;

public abstract class BaseProfileEditor<TProfile, TStore> : BasePage
    where TProfile : ObjectProfile, new()
    where TStore : IObjectProfileStore<TProfile>
{
    private bool _cancelled;
    [Inject] public TStore ProfileStore { get; set; } = default!;
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? ParentName { get; set; }
    protected TProfile Profile { get; set; } = default!;
    protected bool IsValid { get; set; }
    protected bool IsTouched { get; set; }
    protected bool IsNew { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _cancelled = false;
    }

    protected void Cancel()
    {
        _cancelled = true;
        NavigationManager.NavigateToProfileList<TProfile>(ParentName);
    }

    protected override async Task OnSafeParametersSetAsync()
    {
        await base.OnSafeParametersSetAsync();
        await Refresh();
    }

    protected async Task Refresh()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Cancel();
            return;
        }

        var profile = await ProfileStore.TryGet(Name, ParentName);
        IsNew = profile == null;
        Profile = profile ?? await ProfileStore.Create(Name, ParentName);
    }

    protected virtual async Task Save()
    {
        await SafeActionAsync(async () =>
        {
            ObjectProfile.ValidateName(Name);
            var profile = IsNew ? Profile : await ProfileStore.Require(Name!, ParentName);
            await StoreProfile(profile);
            if (!IsNew && ObjectProfile.Compare(profile, Profile))
                return;
            await ProfileStore.Store(profile);
            IsNew = false;
            Profile = profile;
        });
    }

    protected abstract Task StoreProfile(TProfile save);

    protected async Task SaveAndClose()
    {
        await Save();
        Cancel();
    }

    protected async Task Delete()
    {
        var result = await DialogService.ShowMessageBox($"Delete {ProfileStore.Name} Profile",
            "Are you sure that you would like to delete this profile?", "Yes", "No", "Cancel");
        if (result != true) return;
        await ProfileStore.Delete(Profile.Name, Profile.ParentName);
        Cancel();
    }

    protected async Task Rename()
    {
        var parameters = new DialogParameters
        {
            { nameof(NewProfileModal.Names), await ProfileStore.GetNames() },
            { nameof(NewProfileModal.Name), Profile.Name },
            { nameof(NewProfileModal.OkButton), "Rename" }
        };
        var modal = await DialogService.ShowAsync<NewProfileModal>("Rename " + ProfileStore.Name, parameters);
        var result = await modal.Result;
        if (result.Canceled) return;
        await ProfileStore.Rename(Profile.Name, (string)result.Data, Profile.ParentName);
        Cancel();
    }

    protected async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        if (_cancelled || !IsTouched) return;

        var result = await DialogService.ShowMessageBox($"Changed {ProfileStore.Name} Profile",
            "Would you like to save before leaving?", "Yes, Save Changes", "No, Abandon Changes", "Cancel");

        if (result == true) await Save();
        else if (result == null) context.PreventNavigation();
    }
}