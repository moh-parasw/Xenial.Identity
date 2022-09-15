using DevExpress.Xpo;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class IdentityResources
{
    private async Task Delete(XpoIdentityResource resource)
    {
        var delete = await DialogService.ShowMessageBox("Delete Identity Resource", (MarkupString)$"""
            <ul>
                <li>
                    Do your really want to delete the Identity Resource?
                </li>
                <li>
                    <em>{resource.Name}</em>
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
                await UnitOfWork.DeleteAsync(resource);
                await UnitOfWork.CommitChangesAsync();
                Snackbar.Add($"""
                    <ul>
                        <li>
                            Identity Resource was successfully deleted!
                        </li>
                        <li>
                            <em>{resource.Name}</em>
                        </li>
                    </ul>
                    """, MudBlazor.Severity.Success);
                openDrawer = false;
            }
            catch (Exception ex)
            {
                var errors = ex.Message;

                Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when deleting the Identity Resource!
                        </li>
                        <li>
                            <em>{resource.Name}</em>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }

        await table.ReloadServerData();
    }
}
