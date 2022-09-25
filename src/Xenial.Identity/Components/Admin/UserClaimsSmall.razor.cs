﻿using System.Security.Claims;

using MudBlazor;

namespace Xenial.Identity.Components.Admin;

public partial class UserClaimsSmall
{
    private async Task EditClaim(Claim claim = null)
    {
        var dialog = DialogService.Show<UserClaimDialog>(
            claim == null ? "Add Claim" : "Edit Claim",
            new DialogParameters
            {
                [nameof(UserClaimDialog.Claim)] = claim ?? new Claim("", ""),
                [nameof(UserClaimDialog.IsNewClaim)] = claim == null,
                [nameof(UserClaimDialog.User)] = User,
            },
            new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
            });

        var refresh = await dialog.GetReturnValueAsync<bool?>();
        if (refresh == true)
        {
            await Refresh();
        }
    }

    private async Task DeleteClaim(Claim claim)
    {
        currentClaim = claim;

        var delete = await mbox.Show(new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
        });

        if (delete == true)
        {
            var result = await UserManager.RemoveClaimAsync(User, claim);
            if (result.Succeeded)
            {
                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            Claim was successfully deleted!
                        </li>
                        <li>
                           <em>{claim.Type}</em><br>
                        </li>
                    </ul>
                    """, MudBlazor.Severity.Success);
            }
            else
            {
                var errors = string.Join("\n", result.Errors.Select(e => $"<li>Code: {e.Code}: {e.Description}</li>"));

                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when deleting the claim!
                        </li>
                        <li>
                            <em>{claim.Type}</em><br>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }
        await Refresh();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await Refresh();
    }

    private async Task Refresh()
    {
        claims = await UserManager.GetClaimsAsync(User);
        StateHasChanged();
    }
}
