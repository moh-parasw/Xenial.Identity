using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using Google.Protobuf.WellKnownTypes;

using Microsoft.AspNetCore.Identity;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class ApiDetails
{
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
