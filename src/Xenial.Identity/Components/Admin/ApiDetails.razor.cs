using DevExpress.Data.Filtering;
using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using Google.Protobuf.WellKnownTypes;

using Microsoft.AspNetCore.Identity;

using Xenial.Identity.Xpo.Storage.Models;

using static MudBlazor.CategoryTypes;

namespace Xenial.Identity.Components.Admin;

public partial class ApiDetails
{

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
        await SaveRole();
    }

    private async Task AddResource()
    {
        await UnitOfWork.CommitChangesAsync();
        var newScope = "";
        using (var uow = UnitOfWork.BeginNestedUnitOfWork())
        {
            var scope = new XpoApiScope(uow);
            var dialog = DialogService.Show<ApiResourceDialog>("Add Resource", new MudBlazor.DialogParameters
            {
                [nameof(ApiResourceDialog.Prefix)] = Api.Name,
                [nameof(ApiResourceDialog.UnitOfWork)] = uow,
                [nameof(ApiResourceDialog.Scope)] = scope,
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
                newScope = $"{Api.Name}:{scope.Name}";
                scope.Name = newScope;
                await uow.SaveAsync(scope);
                await uow.CommitChangesAsync();
            }
            else
            {
                uow.DropChanges();
            }
        }
        if (!string.IsNullOrEmpty(newScope))
        {
            Api.Scopes.Add(new XpoApiResourceScope(UnitOfWork)
            {
                Scope = newScope
            });
            await SaveRole();
        }
    }

    private IList<string> IdentityResources { get; set; } = new List<string>();

    private async Task<IEnumerable<string>> SearchUserResources(string x)
    {
        var resources = await UnitOfWork.Query<XpoIdentityResource>().Select(r => r.Name).ToArrayAsync();

        if (string.IsNullOrEmpty(x))
        {
            return resources.Except(IdentityResources);
        }

        return resources.Except(IdentityResources).Where(v => v.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    protected async Task SaveRole()
    {
        try
        {
            foreach (var userClaim in Api.UserClaims.ToList())
            {
                Api.UserClaims.Remove(userClaim);
            }

            Api.UserClaims.AddRange(IdentityResources.Select(userClaim => new XpoApiResourceClaim(UnitOfWork)
            {
                Type = userClaim
            }));

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
