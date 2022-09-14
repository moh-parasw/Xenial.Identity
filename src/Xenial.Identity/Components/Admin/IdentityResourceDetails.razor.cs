namespace Xenial.Identity.Components.Admin;

public partial class IdentityResourceDetails
{
    protected async Task Save()
    {
        try
        {
            await UnitOfWork.SaveAsync(IdentityResource);
            await UnitOfWork.CommitChangesAsync();

            await Refresh();
            Snackbar.Add($"""
                <ul>
                    <li>
                        Role was successfully updated!
                    </li>
                    <li>
                        <em>{IdentityResource.Name}</em>
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
                        There was an error when updating the IdendityResource!
                    </li>
                    <li>
                        <em>{IdentityResource.Name}</em>
                    </li>
                    {errors}
                </ul>
                """, MudBlazor.Severity.Error);
        }

    }
}
