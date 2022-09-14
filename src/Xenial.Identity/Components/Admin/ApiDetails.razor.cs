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
        var newScope = "";
        using (var uow = UnitOfWork.BeginNestedUnitOfWork())
        {
            var scope = new XpoApiScope(uow);
            var dialog = DialogService.Show<ApiResourceDialog>("Add Resource", new MudBlazor.DialogParameters
            {
                [nameof(ApiResourceDialog.Prefix)] = Api.Name,
                [nameof(ApiResourceDialog.UnitOfWork)] = uow,
                [nameof(ApiResourceDialog.Scope)] = scope,
                [nameof(ApiResourceDialog.ParentIdentityResources)] = IdentityResources,
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
                newScope = scope.Name;
                await uow.SaveAsync(scope);
                await uow.CommitChangesAsync();
                if (!string.IsNullOrEmpty(newScope))
                {
                    Api.Scopes.Add(new XpoApiResourceScope(UnitOfWork)
                    {
                        Scope = newScope
                    });
                    await Save();
                }
            }
            else
            {
                uow.DropChanges();
            }
        }
        StateHasChanged();
    }

    private async Task EditScope(XpoApiResourceScope scope)
    {
        await UnitOfWork.CommitChangesAsync();
        using (var uow = UnitOfWork.BeginNestedUnitOfWork())
        {
            var existingScope = uow.Query<XpoApiScope>().Where(s => s.Name == scope.Scope).First();

            var dialog = DialogService.Show<ApiResourceDialog>("Edit Resource", new MudBlazor.DialogParameters
            {
                [nameof(ApiResourceDialog.Prefix)] = Api.Name,
                [nameof(ApiResourceDialog.UnitOfWork)] = uow,
                [nameof(ApiResourceDialog.Scope)] = existingScope,
                [nameof(ApiResourceDialog.ParentIdentityResources)] = IdentityResources,
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
                await uow.SaveAsync(existingScope);
                await uow.CommitChangesAsync();

                scope.Scope = existingScope.Name;
                await UnitOfWork.SaveAsync(scope);
                await Save();
            }
            else
            {
                uow.DropChanges();
            }
        }
        StateHasChanged();
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

    protected async Task Save()
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

            var existingScope = UnitOfWork.Query<XpoApiScope>().Where(s => s.Name == Api.Name).FirstOrDefault() ?? new XpoApiScope(UnitOfWork);
            existingScope.Name = Api.Name;
            existingScope.Description = Api.Description;
            existingScope.DisplayName = Api.DisplayName;
            existingScope.ShowInDiscoveryDocument = Api.ShowInDiscoveryDocument;
            existingScope.Required = Api.Required;
            existingScope.Emphasize = Emphasize;

            foreach (var userClaim in existingScope.UserClaims.ToList())
            {
                existingScope.UserClaims.Remove(userClaim);
            }

            existingScope.UserClaims.AddRange(IdentityResources.Select(userClaim => new XpoApiScopeClaim(UnitOfWork)
            {
                Type = userClaim
            }));

            await UnitOfWork.SaveAsync(existingScope);
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
