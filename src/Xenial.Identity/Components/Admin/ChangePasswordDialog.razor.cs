using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using MudBlazor;

using Xenial.Identity.Data;

namespace Xenial.Identity.Components.Admin;

public partial class ChangePasswordDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter, EditorRequired]
    public XenialIdentityUser User { get; set; }
    private string Password { get; set; }
    private string RepeatPassword { get; set; }

    protected void Cancel()
        => MudDialog?.Cancel();

    protected async Task Save()
    {
        ShowSnackbarIfError(await UserManager.RemovePasswordAsync(User), "removing the password");

        if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(RepeatPassword))
        {
            ShowSnackbarIfError(IdentityResult.Failed(new IdentityError { Code = "0000", Description = "Passwords must not be empty" }));
            return;
        }

        if (Password != RepeatPassword)
        {
            ShowSnackbarIfError(IdentityResult.Failed(new IdentityError { Code = "0001", Description = "Passwords must match" }));
            return;
        }

        var result = await UserManager.AddPasswordAsync(User, Password);

        ShowSnackback(result);

        void ShowSnackback(IdentityResult result)
        {
            if (result.Succeeded)
            {
                _ = Snackbar.Add($"""
                <ul>
                    <li>
                        Password was successfully changed!
                    </li>
                    <li>
                        <em>{User.UserName}</em>
                    </li>
                </ul>
                """, MudBlazor.Severity.Success);
                MudDialog.Close(true);
            }
            else
            {
                ShowSnackbarIfError(result);
            }
        }

        void ShowSnackbarIfError(IdentityResult result, string message = "changing the password")
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("\n", result.Errors.Select(e => $"<li>Code: {e.Code}: {e.Description}</li>"));

            _ = Snackbar.Add($"""
                <ul>
                    <li>
                        There was an error when {message}!
                    </li>
                    <li>
                        <em>{User.UserName}</em>
                    </li>
                    {errors}
                </ul>
                """, MudBlazor.Severity.Error);
        }
    }
}
