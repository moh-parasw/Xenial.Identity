using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using Xenial.Identity.Data;

namespace Xenial.Identity.Components.Admin;

public partial class Apis
{
    private async Task DeleteRole(IdentityRole role)
    {
        var delete = await DialogService.ShowMessageBox("Delete Role", (MarkupString)$"""
            <ul>
                <li>
                    Do your really want to delete the role?
                </li>
                <li>
                    <em>{role.Name}</em>
                </li>
                <li>
                    <strong>This operation can <em>not</em> be undone!</strong>
                </li>
            </ul>
        """, yesText: "Delete!", cancelText: "Cancel");

        if (delete == true)
        {
            var result = await RolesManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                Snackbar.Add($"""
                    <ul>
                        <li>
                            Role was successfully deleted!
                        </li>
                        <li>
                            <em>{role.Name}</em>
                        </li>
                    </ul>
                    """, MudBlazor.Severity.Success);
            }
            else
            {
                var errors = string.Join("\n", result.Errors.Select(e => $"<li>Code: {e.Code}: {e.Description}</li>"));

                Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when deleting the role!
                        </li>
                        <li>
                            <em>{role.Name}</em>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }

        await table.ReloadServerData();
    }
}
