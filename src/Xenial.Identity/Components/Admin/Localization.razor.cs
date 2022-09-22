using Google.Protobuf.WellKnownTypes;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using Xenial.Identity.Data;
using Xenial.Identity.Models;

using static System.Formats.Asn1.AsnWriter;

namespace Xenial.Identity.Components.Admin;

public partial class Localization
{
    private async Task Add()
    {
        using var childUow = UOW.BeginNestedUnitOfWork();
        var loc = new XpoLocalization(childUow);
        var dialog = DialogService.Show<LocalizationDialog>("Add Localization", new MudBlazor.DialogParameters
        {
            [nameof(LocalizationDialog.UnitOfWork)] = childUow,
            [nameof(LocalizationDialog.Localization)] = loc,
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
                Snackbar.Add($"""
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

                Snackbar.Add($"""
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
