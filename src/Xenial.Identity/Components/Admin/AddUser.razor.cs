using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;

using Xenial.Identity.Data;

namespace Xenial.Identity.Components.Admin;

public partial class AddUser
{
    private string Password { get; set; }
    private string RepeatPassword { get; set; }

    protected async Task<IdentityResult> ValidatePasswordAsync(XenialIdentityUser user, string password)
    {
        var errors = new List<IdentityError>();
        var isValid = true;
        foreach (var v in UserManager.PasswordValidators)
        {
            var result = await v.ValidateAsync(UserManager, user, password);
            if (!result.Succeeded)
            {
                if (result.Errors.Any())
                {
                    errors.AddRange(result.Errors);
                }

                isValid = false;
            }
        }
        if (!isValid)
        {
            return IdentityResult.Failed(errors.ToArray());
        }
        return IdentityResult.Success;
    }

    protected async Task SaveUser()
    {
        if (string.IsNullOrWhiteSpace(User.UserName))
        {
            ShowSnackbarIfError(IdentityResult.Failed(new IdentityError { Code = "0000", Description = "Username must not be empty" }));
            return;
        }

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
        var validatePassword = await ValidatePasswordAsync(User, Password);
        if (!validatePassword.Succeeded)
        {
            ShowSnackbarIfError(validatePassword, "validating the password");
            return;
        }
        var userResult = await UserManager.CreateAsync(User);
        if (!userResult.Succeeded)
        {
            ShowSnackbarIfError(userResult);
            return;
        }

        var result = await UserManager.AddPasswordAsync(User, Password);

        await ShowSnackback(result);

        async Task ShowSnackback(IdentityResult result)
        {
            if (result.Succeeded)
            {
                Snackbar.Add($"""
                <ul>
                    <li>
                        User was successfully created!
                    </li>
                    <li>
                        <em>{User.UserName}</em>
                    </li>
                </ul>
                """, MudBlazor.Severity.Success);

                var userId = await UserManager.GetUserIdAsync(User);
                NavigationManager.NavigateTo($"/Admin2/User/{userId}");
            }
            else
            {
                ShowSnackbarIfError(result);
            }
        }

        void ShowSnackbarIfError(IdentityResult result, string message = "creating the user")
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
                        <em>{User.UserName}</em>
                    </li>
                    {errors}
                </ul>
                """, MudBlazor.Severity.Error);
        }
    }
}
