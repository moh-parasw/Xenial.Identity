using Microsoft.AspNetCore.Identity;

namespace Xenial.Identity.Components.Admin;

public partial class AddUser
{
    protected async Task SaveUser()
    {
        var result = await UserManager.CreateAsync(User);

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
