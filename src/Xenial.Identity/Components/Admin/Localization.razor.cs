using System.ComponentModel.DataAnnotations;
using System.Reflection;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Components;

using Xenial.Identity.Models;

using XLocalizer;

namespace Xenial.Identity.Components.Admin;

public partial class Localization
{
    private async Task<IEnumerable<string>> SearchFunc(string x)
    {
        static IEnumerable<string> GetXLocalizerItems(object node)
        {
            return node.GetType()
                        .GetProperties()
                        .Where(x =>
                            x.PropertyType == typeof(string)
                            && x.GetCustomAttribute<RequiredAttribute>() is not null
                        )
                        .Select(x => x.GetValue(node))
                        .OfType<string>();
        }

        var existingKeys = await UOW
            .Query<XpoLocalization>()
            .Select(m => m.Key)
            .ToListAsync();

        var options = new XLocalizerOptions();
        var xlocalizerItems = GetXLocalizerItems(options.ValidationErrors)
            .Concat(GetXLocalizerItems(options.ModelBindingErrors))
            .Concat(GetXLocalizerItems(options.IdentityErrors))
            .Where(L.IsUnmatched);

        var keys = L.UnmatchedLocalizations
            .Concat(xlocalizerItems)
            .Except(existingKeys)
            .Distinct();

        return string.IsNullOrEmpty(x) ? keys : keys.Where(k => k.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }
    private async Task<bool> Validate(string x)
    {
        if (string.IsNullOrEmpty(x))
        {
            _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when adding the localization!
                        </li>
                        <li>
                          <em>Key must not be empty!</em><br>
                        </li>
                    </ul>
                    """, MudBlazor.Severity.Error);
            return false;
        }

        var existingKeys = await UOW.Query<XpoLocalization>().Select(m => m.Key).ToListAsync();
        var contains = existingKeys.Contains(x);
        if (contains)
        {
            _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when adding the localization!
                        </li>
                        <li>
                          <em>Key: {x} does already exist!</em><br>
                        </li>
                    </ul>
                    """, MudBlazor.Severity.Error);
        }
        return !contains;
    }

    private async Task Add()
    {
        using var childUow = UOW.BeginNestedUnitOfWork();
        var loc = new XpoLocalization(childUow);
        var dialog = DialogService.Show<LocalizationDialog>("Add Localization", new MudBlazor.DialogParameters
        {
            [nameof(LocalizationDialog.UnitOfWork)] = childUow,
            [nameof(LocalizationDialog.Localization)] = loc,
            [nameof(LocalizationDialog.SearchFunc)] = new Func<string, Task<IEnumerable<string>>>(SearchFunc),
            [nameof(LocalizationDialog.Validate)] = new Func<string, Task<bool>>(Validate),
        }, new MudBlazor.DialogOptions
        {
            MaxWidth = MudBlazor.MaxWidth.Small,
            FullWidth = true,
            Position = MudBlazor.DialogPosition.TopCenter,
            NoHeader = false,
            CloseButton = true,
            CloseOnEscapeKey = true
        });

        var result = await dialog.GetReturnValueAsync<bool?>();
        if (result == true)
        {
            await childUow.SaveAsync(loc);
            await childUow.CommitChangesAsync();
            await UOW.CommitChangesAsync();
        }
        else
        {
            childUow.DropChanges();
        }
        await LocalizerService.Refresh();
        await table.ReloadServerData();
    }

    private async Task Edit(XpoLocalization loc)
    {
        using var childUow = UOW.BeginNestedUnitOfWork();
        var loc2 = childUow.GetObjectByKey<XpoLocalization>(loc.Id);
        var dialog = DialogService.Show<LocalizationDialog>("Edit Localization", new MudBlazor.DialogParameters
        {
            [nameof(LocalizationDialog.UnitOfWork)] = childUow,
            [nameof(LocalizationDialog.Localization)] = loc2,
            [nameof(LocalizationDialog.AllowEditKey)] = false
        }, new MudBlazor.DialogOptions
        {
            MaxWidth = MudBlazor.MaxWidth.Small,
            FullWidth = true,
            Position = MudBlazor.DialogPosition.TopCenter,
            NoHeader = false,
            CloseButton = true,
            CloseOnEscapeKey = true
        });

        var result = await dialog.GetReturnValueAsync<bool?>();
        if (result == true)
        {
            await childUow.SaveAsync(loc2);
            await childUow.CommitChangesAsync();
            await UOW.CommitChangesAsync();
        }
        else
        {
            childUow.DropChanges();
        }
        await LocalizerService.Refresh();
        await table.ReloadServerData();
    }

    private async Task Delete(XpoLocalization localization)
    {
        await Task.CompletedTask;
        await Task.CompletedTask;
        var delete = await DialogService.ShowMessageBox("Delete Localization", (MarkupString)$"""
            <ul>
                <li>
                    Do your really want to delete the localization?
                </li>
                <li>
                    <em>{localization.Key}</em><br>
                    <em>{localization.Value}</em>
                </li>
                <li>
                    <strong>This operation can <em>not</em> be undone!</strong>
                </li>
            </ul>
        """, yesText: "Delete!", cancelText: "Cancel");

        if (delete == true)
        {
            try
            {
                await UOW.DeleteAsync(localization);
                await UOW.CommitChangesAsync();
                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            Localization was successfully deleted!
                        </li>
                        <li>
                            <em>{localization.Key}</em><br>
                            <em>{localization.Value}</em>
                        </li>
                    </ul>
                    """, MudBlazor.Severity.Success);
            }
            catch (Exception ex)
            {
                var errors = ex.Message;

                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when deleting the localization!
                        </li>
                        <li>
                          <em>{localization.Key}</em><br>
                            <em>{localization.Value}</em>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }
        await LocalizerService.Refresh();
        await table.ReloadServerData();
    }
}
