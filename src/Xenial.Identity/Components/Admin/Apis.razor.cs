using DevExpress.Xpo;

using Microsoft.AspNetCore.Components;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class Apis
{
    private async Task Delete(XpoApiResource resource)
    {
        var delete = await DialogService.ShowMessageBox("Delete API", (MarkupString)$"""
            <ul>
                <li>
                    Do your really want to delete the API?
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
                foreach (var scopeName in resource.Scopes.Select(m => m.Scope))
                {
                    var scopes = await UnitOfWork.Query<XpoApiScope>().Where(x => x.Name == scopeName).ToListAsync();
                    if (scopes.Count > 0)
                    {
                        foreach (var item in scopes)
                        {
                            await UnitOfWork.DeleteAsync(item);
                        }
                    }
                }

                var scope = await UnitOfWork.Query<XpoApiScope>().Where(x => x.Name == resource.Name).FirstOrDefaultAsync();
                if (scope is not null)
                {
                    await UnitOfWork.DeleteAsync(scope);
                }

                await UnitOfWork.DeleteAsync(resource);
                await UnitOfWork.CommitChangesAsync();
                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            API was successfully deleted!
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

                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when deleting the API!
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
