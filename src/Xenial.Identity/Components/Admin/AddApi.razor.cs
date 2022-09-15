using DevExpress.Xpo;

namespace Xenial.Identity.Components.Admin;

public partial class AddApi
{
    public async Task Save()
    {
        try
        {
            await UnitOfWork.SaveAsync(Api);
            await UnitOfWork.CommitChangesAsync();
            Snackbar.Add($"""
                <ul>
                    <li>
                        Api was successfully created!
                    </li>
                    <li>
                        <em>{Api.Name}</em>
                    </li>
                </ul>
                """, MudBlazor.Severity.Success);
            NavigationManager.NavigateTo($"/Admin2/Api/{Api.Id}");
        }
        catch (Exception ex)
        {
            var errors = ex.Message;

            Snackbar.Add($"""
                <ul>
                    <li>
                        There was an error when creating the Api!
                    </li>
                    <li>
                        <em>{Api.Name}</em>
                    </li>
                    {errors}
                </ul>
                """, MudBlazor.Severity.Error);
        }
    }
}
