using DevExpress.Xpo;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class ApiSecrets
{
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
                Snackbar.Add($"""
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

                Snackbar.Add($"""
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
