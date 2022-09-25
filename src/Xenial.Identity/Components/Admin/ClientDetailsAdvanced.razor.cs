using DevExpress.Xpo;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class ClientDetailsAdvanced
{
    private IEnumerable<XpoApiResourceScope> Scopes { get; set; } = Enumerable.Empty<XpoApiResourceScope>();

    private async Task DeleteScope(XpoApiResourceScope scope)
    {
        var scopeName = scope.Scope;
        await UnitOfWork.DeleteAsync(scope);
        var scopes = await UnitOfWork.Query<XpoApiScope>().Where(x => x.Name == scopeName).ToListAsync();
        if (scopes.Count > 0)
        {
            foreach (var item in scopes)
            {
                await UnitOfWork.DeleteAsync(item);
            }
        }
        await Save();
    }

    private async Task AddScope()
    {
        await UnitOfWork.CommitChangesAsync();
        //var newScope = "";
        //using (var uow = UnitOfWork.BeginNestedUnitOfWork())
        //{
        //    var scope = new XpoApiScope(uow);
        //    var dialog = DialogService.Show<ApiResourceDialog>("Add Resource", new MudBlazor.DialogParameters
        //    {
        //        [nameof(ApiResourceDialog.Prefix)] = Client.Name,
        //        [nameof(ApiResourceDialog.UnitOfWork)] = uow,
        //        [nameof(ApiResourceDialog.Scope)] = scope,
        //        [nameof(ApiResourceDialog.ParentIdentityResources)] = IdentityResources,
        //    }, new MudBlazor.DialogOptions
        //    {
        //        MaxWidth = MudBlazor.MaxWidth.Small,
        //        FullWidth = true,
        //        Position = MudBlazor.DialogPosition.TopCenter,
        //        NoHeader = false,
        //        CloseButton = true,
        //        CloseOnEscapeKey = true
        //    });

        //    if ((await dialog.GetReturnValueAsync<bool?>()) ?? false)
        //    {
        //        newScope = scope.Name;
        //        await uow.SaveAsync(scope);
        //        await uow.CommitChangesAsync();
        //        if (!string.IsNullOrEmpty(newScope))
        //        {
        //            Client.Scopes.Add(new XpoApiResourceScope(UnitOfWork)
        //            {
        //                Scope = newScope
        //            });
        //            await Save();
        //        }
        //    }
        //    else
        //    {
        //        uow.DropChanges();
        //    }
        //}
        StateHasChanged();
    }

    private async Task EditScope(XpoApiResourceScope scope)
    {
        await UnitOfWork.CommitChangesAsync();
        //using (var uow = UnitOfWork.BeginNestedUnitOfWork())
        //{
        //    var existingScope = uow.Query<XpoApiScope>().Where(s => s.Name == scope.Scope).First();

        //    var dialog = DialogService.Show<ApiResourceDialog>("Edit Resource", new MudBlazor.DialogParameters
        //    {
        //        [nameof(ApiResourceDialog.Prefix)] = Client.Name,
        //        [nameof(ApiResourceDialog.UnitOfWork)] = uow,
        //        [nameof(ApiResourceDialog.Scope)] = existingScope,
        //        [nameof(ApiResourceDialog.ParentIdentityResources)] = IdentityResources,
        //    }, new MudBlazor.DialogOptions
        //    {
        //        MaxWidth = MudBlazor.MaxWidth.Small,
        //        FullWidth = true,
        //        Position = MudBlazor.DialogPosition.TopCenter,
        //        NoHeader = false,
        //        CloseButton = true,
        //        CloseOnEscapeKey = true
        //    });

        //    if ((await dialog.GetReturnValueAsync<bool?>()) ?? false)
        //    {
        //        await uow.SaveAsync(existingScope);
        //        await uow.CommitChangesAsync();

        //        scope.Scope = existingScope.Name;
        //        await UnitOfWork.SaveAsync(scope);
        //        await Save();
        //    }
        //    else
        //    {
        //        uow.DropChanges();
        //    }
        //}
        StateHasChanged();
    }

    private IList<string> IdentityResources { get; set; } = new List<string>();

    private async Task<IEnumerable<string>> SearchUserResources(string x)
    {
        var resources = await UnitOfWork.Query<XpoIdentityResource>().Select(r => r.Name).ToArrayAsync();

        return string.IsNullOrEmpty(x)
            ? resources.Except(IdentityResources)
            : resources.Except(IdentityResources).Where(v => v.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task Save()
    {
        try
        {
            //foreach (var userClaim in Client.UserClaims.ToList())
            //{
            //    Client.UserClaims.Remove(userClaim);
            //}

            //Client.UserClaims.AddRange(IdentityResources.Select(userClaim => new XpoApiResourceClaim(UnitOfWork)
            //{
            //    Type = userClaim
            //}));

            //var scope = Client.Scopes.FirstOrDefault(s => s.Scope == Client.Name) ?? new XpoApiResourceScope(UnitOfWork) { Scope = Client.Name, ApiResource = Client };
            //await UnitOfWork.SaveAsync(scope);

            //var existingScope = UnitOfWork.Query<XpoApiScope>().Where(s => s.Name == Client.Name).FirstOrDefault() ?? new XpoApiScope(UnitOfWork);
            //existingScope.Name = Client.Name;
            //existingScope.Description = Client.Description;
            //existingScope.DisplayName = Client.DisplayName;
            //existingScope.ShowInDiscoveryDocument = Client.ShowInDiscoveryDocument;
            //existingScope.Required = Client.Required;
            //existingScope.Emphasize = Emphasize;

            //foreach (var userClaim in existingScope.UserClaims.ToList())
            //{
            //    existingScope.UserClaims.Remove(userClaim);
            //}

            //existingScope.UserClaims.AddRange(IdentityResources.Select(userClaim => new XpoApiScopeClaim(UnitOfWork)
            //{
            //    Type = userClaim
            //}));

            //await UnitOfWork.SaveAsync(existingScope);
            //await UnitOfWork.SaveAsync(Client);
            await UnitOfWork.CommitChangesAsync();
            //await Reload();
            //Snackbar.Add($"""
            //    <ul>
            //        <li>
            //            Api was successfully updated!
            //        </li>
            //        <li>
            //            <em>{Client.Name}</em>
            //        </li>
            //    </ul>
            //    """, MudBlazor.Severity.Success);
        }
        catch (Exception ex)
        {
            _ = ex.Message;

            //Snackbar.Add($"""
            //    <ul>
            //        <li>
            //            There was an error when updating the Api!
            //        </li>
            //        <li>
            //            <em>{Client.Name}</em>
            //        </li>
            //        {errors}
            //    </ul>
            //    """, MudBlazor.Severity.Error);
        }
    }
}
