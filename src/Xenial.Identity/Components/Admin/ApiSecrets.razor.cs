using DevExpress.Xpo;

using Microsoft.AspNetCore.Components;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class ApiSecrets
{
    [CascadingParameter]
    private ApiDetails ApiDetails { get; set; }

    private async Task AddSecret()
    {
        await UnitOfWork.CommitChangesAsync();
        using (var uow = UnitOfWork.BeginNestedUnitOfWork())
        {
            var resource = uow.GetObjectByKey<XpoApiResource>(Resource.Id);
            var secret = new XpoApiResourceSecret(uow)
            {
                ApiResource = resource,
                Value = IdentityModel.CryptoRandom.CreateUniqueId()
            };
            var dialog = DialogService.Show<AddApiSecretDialog>("Add Secret", new MudBlazor.DialogParameters
            {
                [nameof(AddApiSecretDialog.UnitOfWork)] = uow,
                [nameof(AddApiSecretDialog.Secret)] = secret,
            }, new MudBlazor.DialogOptions
            {
                MaxWidth = MudBlazor.MaxWidth.Small,
                FullWidth = true,
                Position = MudBlazor.DialogPosition.TopCenter,
                NoHeader = false,
                CloseButton = true,
                CloseOnEscapeKey = true
            });

            if ((await dialog.GetReturnValueAsync<bool?>()) ?? false)
            {
                await uow.SaveAsync(secret);
                await uow.CommitChangesAsync();
                await ApiDetails.Save();
                await Reload();
            }
            else
            {
                uow.DropChanges();
            }
        }
        StateHasChanged();
    }

    private async Task Delete(XpoApiResourceSecret secret)
    {
        var delete = await DialogService.ShowMessageBox("Delete Secret", (MarkupString)$"""
            <ul>
                <li>
                    Do your really want to delete the secret?
                </li>
                <li>
                    <em>{secret.Id}</em>
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
                await UnitOfWork.DeleteAsync(secret);
                await UnitOfWork.CommitChangesAsync();
                await Reload();
                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            API was successfully deleted!
                        </li>
                        <li>
                            <em>{secret.Id}</em>
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
                            There was an error when deleting the secret!
                        </li>
                        <li>
                            <em>{secret.Id}</em>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }
    }
}
