namespace Xenial.Identity.Components.Admin;

public partial class ApiDetails
{
    protected async Task SaveRole()
    {
        try
        {
            await UnitOfWork.SaveAsync(Api);
            await UnitOfWork.CommitChangesAsync();
            await Reload();
            Snackbar.Add($"""
                <ul>
                    <li>
                        Api was successfully updated!
                    </li>
                    <li>
                        <em>{Api.Name}</em>
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
                        There was an error when updating the Api!
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
