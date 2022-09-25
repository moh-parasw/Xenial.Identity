﻿using System.Security.Claims;

using DevExpress.Xpo;

using IdentityModel;

using Microsoft.AspNetCore.Components.Forms;

using Microsoft.AspNetCore.Identity;

using Newtonsoft.Json;

using Xenial.Identity.Infrastructure;

namespace Xenial.Identity.Components.Admin;

public partial class UserDetails
{
    private async Task<IEnumerable<string>> SearchRoles(string x)
    {
        var roles = await RolesManager.Roles.Select(x => x.Name).ToListAsync();
        return string.IsNullOrEmpty(x)
            ? roles.Except(Roles)
            : roles.Except(Roles).Where(v => v.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    protected async Task SaveUser()
    {
        var existingUser = await UserManager.FindByIdAsync(User.Id);
        if (existingUser is not null)
        {
            if (existingUser.PhoneNumber != User.PhoneNumber)
            {
                ShowSnackbarIfError(await UserManager.SetPhoneNumberAsync(User, User.PhoneNumber), "updating the phonenumber");
            }
            if (existingUser.Email != User.Email)
            {
                ShowSnackbarIfError(await UserManager.SetEmailAsync(User, User.Email), "updating the email");
            }
            if (existingUser.PhoneNumberConfirmed != User.PhoneNumberConfirmed)
            {
                if (User.PhoneNumberConfirmed)
                {
                    var token = await UserManager.GenerateChangePhoneNumberTokenAsync(User, User.PhoneNumber);
                    ShowSnackbarIfError(await UserManager.ChangePhoneNumberAsync(User, User.PhoneNumber, token), "confirming the phone number");
                }
            }
            if (existingUser.EmailConfirmed != User.EmailConfirmed)
            {
                if (User.EmailConfirmed)
                {
                    var token = await UserManager.GenerateEmailConfirmationTokenAsync(User);
                    ShowSnackbarIfError(await UserManager.ConfirmEmailAsync(User, token), "confirming the email address");
                }
            }
            var existingRoles = await UserManager.GetRolesAsync(existingUser);
            if (!existingRoles.SequenceEqual(Roles))
            {
                ShowSnackbarIfError(await UserManager.RemoveFromRolesAsync(User, existingRoles), "removing roles");
                ShowSnackbarIfError(await UserManager.AddToRolesAsync(User, Roles), "adding roles");
            }
        }

        await UserManager.SetOrUpdateClaimAsync(User, new Claim("name", User.FullName ?? string.Empty));
        await UserManager.SetOrUpdateClaimAsync(User, new Claim("family_name", User.LastName ?? string.Empty));
        await UserManager.SetOrUpdateClaimAsync(User, new Claim("given_name", User.FirstName ?? string.Empty));
        //await SetOrUpdateClaimAsync(User, new Claim("website", User.Website ?? string.Empty));
        //await SetOrUpdateClaimAsync(User, new Claim("profile", absProfileUrl));

        //TODO: Map if needed
        //await SetOrUpdateClaimAsync(User, new Claim("gender", User.Gender ?? string.Empty));
        //await SetOrUpdateClaimAsync(User, new Claim("birthdate", User.Birthdate?.ToString("YYYY-MM-DD") ?? string.Empty));
        //await SetOrUpdateClaimAsync(User, new Claim("zoneinfo", User.Zoneinfo ?? string.Empty));
        //await SetOrUpdateClaimAsync(User, new Claim("locale", User.Locale ?? string.Empty));
        await UserManager.SetOrUpdateClaimAsync(User, new Claim("updated_at", ConvertToUnixTimestamp(User.UpdatedAt)?.ToString() ?? string.Empty));
        await UserManager.SetOrUpdateClaimAsync(User, new Claim("xenial_backcolor", User.Color ?? string.Empty));
        await UserManager.SetOrUpdateClaimAsync(User, new Claim("xenial_forecolor", (MaterialColorPicker.ColorIsDark(User.Color) ? "#FFFFFF" : "#000000") ?? string.Empty));
        await UserManager.SetOrUpdateClaimAsync(User, new Claim("xenial_initials", User.Initials ?? string.Empty));

        var streetAddress = string.Join(" ", new[] { User.AddressStreetAddress1, User.AddressStreetAddress2 }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var postalAddress = string.Join(" ", new[] { User.AddressPostalCode, User.AddressLocality }.Where(s => !string.IsNullOrWhiteSpace(s)));

        await UserManager.SetOrUpdateClaimAsync(User, new Claim("address", JsonConvert.SerializeObject(new
        {
            formatted = string.Join(Environment.NewLine, new[] { streetAddress, postalAddress, User.AddressRegion, User.AddressCountry }.Where(s => !string.IsNullOrWhiteSpace(s))) ?? string.Empty,
            street_address = streetAddress ?? string.Empty,
            locality = User.AddressLocality ?? string.Empty,
            region = User.AddressRegion ?? string.Empty,
            postal_code = User.AddressPostalCode ?? string.Empty,
            country = User.AddressCountry ?? string.Empty,
        }, Formatting.Indented)));

        User.UpdatedAt = DateTime.Now;
        var result = await UserManager.UpdateAsync(User);

        await ShowSnackback(result);

        static double? ConvertToUnixTimestamp(DateTime? date)
        {
            if (date.HasValue)
            {
                var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var diff = date.Value.ToUniversalTime() - origin;
                return Math.Floor(diff.TotalSeconds);
            }
            return null;
        }

        async Task ShowSnackback(IdentityResult result)
        {
            if (result.Succeeded)
            {
                await ReloadUser();
                _ = Snackbar.Add($"""
                <ul>
                    <li>
                        User was successfully updated!
                    </li>
                    <li>
                        <em>{User.UserName}</em>
                    </li>
                </ul>
                """, MudBlazor.Severity.Success);
            }
            else
            {
                ShowSnackbarIfError(result);
            }
        }

        void ShowSnackbarIfError(IdentityResult result, string message = "updating the user")
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

    private async Task UploadProfilePicture(InputFileChangeEventArgs e)
    {
        using var reader = e.File.OpenReadStream();
        using var ms = new MemoryStream();
        await reader.CopyToAsync(ms);
        User.PictureMimeType = e.File.ContentType;
        User.Picture = ms.ToArray();

        if (string.IsNullOrEmpty(User.PictureId)) //We only set a new one if there wasn't one before
        {
            User.PictureId = CryptoRandom.CreateUniqueId();
        }

        string AbsolutePictureUri(string pictureId)
        {
            return $"{NavigationManager.BaseUri}api/profile/picture/{pictureId}";
        }

        var picture = AbsolutePictureUri(User.PictureId);
        await UserManager.SetOrUpdateClaimAsync(User, new System.Security.Claims.Claim("picture", picture));

        var result = await UserManager.UpdateAsync(User);

        if (result.Succeeded)
        {
            await ReloadUser();
            _ = Snackbar.Add($"""
                <ul>
                    <li>
                        Profile picture was successfully updated!
                    </li>
                    <li>
                        <em>{User.UserName}</em>
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
                        There was an error when updating the profile picture!
                    </li>
                    <li>
                        <em>{User.UserName}</em>
                    </li>
                    {errors}
                </ul>
                """, MudBlazor.Severity.Error);
        }
    }

    private async Task DeleteProfilePicture()
    {
        User.Picture = new byte[0];
        User.PictureMimeType = null;
        //Don't reset PictureId. Should stay static as long user lives cause tokens are cached
        //user.PictureId = null;
        await UserManager.RemoveClaimAsync(User, "picture");

        var result = await UserManager.UpdateAsync(User);
        if (result.Succeeded)
        {
            await ReloadUser();
            _ = Snackbar.Add($"""
                <ul>
                    <li>
                        Profile picture was successfully removed!
                    </li>
                    <li>
                        <em>{User.UserName}</em>
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
                        There was an error when removing the profile picture!
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
