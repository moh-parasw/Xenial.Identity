using IdentityModel;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;

using MudBlazor.Utilities;

using Newtonsoft.Json;

using System.IO;
using System.Security.Claims;

using TextMateSharp.Themes;

using Xenial.Identity.Infrastructure;

namespace Xenial.Identity.Components.Admin;

public partial class AddRole
{
    protected async Task SaveRole()
    {
        var result = await RolesManager.CreateAsync(Role);

        await ShowSnackback(result);

        async Task ShowSnackback(IdentityResult result)
        {
            if (result.Succeeded)
            {
                Snackbar.Add($"""
                <ul>
                    <li>
                        Role was successfully created!
                    </li>
                    <li>
                        <em>{Role.Name}</em>
                    </li>
                </ul>
                """, MudBlazor.Severity.Success);

                var roleId = await RolesManager.GetRoleIdAsync(Role);
                NavigationManager.NavigateTo($"/Admin2/Role/{roleId}");
            }
            else
            {
                ShowSnackbarIfError(result);
            }
        }

        void ShowSnackbarIfError(IdentityResult result, string message = "creating the role")
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("\n", result.Errors.Select(e => $"<li>Code: {e.Code}: {e.Description}</li>"));

            Snackbar.Add($"""
                <ul>
                    <li>
                        There was an error when {message}!
                    </li>
                    <li>
                        <em>{Role.Name}</em>
                    </li>
                    {errors}
                </ul>
                """, MudBlazor.Severity.Error);
        }
    }
}
