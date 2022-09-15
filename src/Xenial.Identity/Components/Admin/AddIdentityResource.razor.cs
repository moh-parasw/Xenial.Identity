namespace Xenial.Identity.Components.Admin;

public partial class AddIdentityResource
{
    protected async Task Save()
    {
        try
        {
            await UnitOfWork.SaveAsync(IdentityResource);
            await UnitOfWork.CommitChangesAsync();

            Snackbar.Add($"""
                <ul>
                    <li>
                        IdentityResource was successfully added!
                    </li>
                    <li>
                        <em>{IdentityResource.Name}</em>
                    </li>
                </ul>
                """, MudBlazor.Severity.Success);
            NavigationManager.NavigateTo($"/Admin2/IdentityResource/{IdentityResource.Id}");
        }
        catch (Exception ex)
        {
            var errors = ex.Message;
            Snackbar.Add($"""
                <ul>
                    <li>
                        There was an error when adding the IdendityResource!
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
