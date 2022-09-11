using Microsoft.AspNetCore.Components;

using Xenial.Identity.Data;

namespace Xenial.Identity.Components.Admin;

public partial class Users
{
    private async Task DeleteUser(XenialIdentityUser user)
    {
        var delete = await DialogService.ShowMessageBox("Delete User", (MarkupString)$"""
            <ul>
                <li>
                    Do your really want to delete the user?
                </li>
                <li>
                    <em>{user.UserName}</em>
                </li>
                <li>
                    <strong>This operation can <em>not</em> be undone!</strong>
                </li>
            </ul>
        """, yesText: "Delete!", cancelText: "Cancel");

        if (delete == true)
        {
            var result = await UserManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                Snackbar.Add($"""
                    <ul>
                        <li>
                            User was successfully deleted!
                        </li>
                        <li>
                            <em>{user.UserName}</em>
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
                            There was an error when deleting the user!
                        </li>
                        <li>
                            <em>{user.UserName}</em>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }

        await this.table.ReloadServerData();
    }
}
