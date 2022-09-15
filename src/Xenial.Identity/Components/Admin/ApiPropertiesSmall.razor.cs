using System.Security.Claims;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class ApiPropertiesSmall
{
    private async Task Edit(XpoApiResourceProperty property = null)
    {
        using (var uow = UnitOfWork.BeginNestedUnitOfWork())
        {
            var resource = uow.GetObjectByKey<XpoApiResource>(Resource.Id);
            var currentProperty = property switch
            {
                { } => uow.GetObjectByKey<XpoApiResourceProperty>(property.Id),
                _ => new XpoApiResourceProperty(uow) { ApiResource = resource }
            };
            var dialog = DialogService.Show<ApiPropertiesDialog>(
                property == null ? "Add Property" : "Edit Property",
                new DialogParameters
                {
                    [nameof(ApiPropertiesDialog.Property)] = currentProperty,
                    [nameof(ApiPropertiesDialog.IsNew)] = property == null,
                    [nameof(ApiPropertiesDialog.Resource)] = resource,
                },
                new DialogOptions
                {
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true,
                    CloseOnEscapeKey = true,
                    DisableBackdropClick = true,
                });

            var refresh = await dialog.GetReturnValueAsync<bool?>();
            if (refresh == true)
            {
                await uow.SaveAsync(currentProperty);
                await uow.CommitChangesAsync();
                await UnitOfWork.SaveAsync(Resource);
                await UnitOfWork.CommitChangesAsync();
                await Refresh();
            }
            else
            {
                uow.DropChanges();
            }
        }
    }

    private async Task Delete(XpoApiResourceProperty property)
    {
        currentProperty = property;

        var delete = await mbox.Show(new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
        });

        if (delete == true)
        {
            try
            {
                await UnitOfWork.DeleteAsync(property);
                await UnitOfWork.CommitChangesAsync();

                Snackbar.Add($"""
                    <ul>
                        <li>
                            Property was successfully deleted!
                        </li>
                        <li>
                           <em>{property.Key}</em><br>
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
                            There was an error when deleting the property!
                        </li>
                        <li>
                            <em>{property.Key}</em><br>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }
        await Refresh();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await Refresh();
    }

    private async Task Refresh()
    {
        if (Resource.IsSaveForBusinessLogic)
        {
            Resource.Properties.Reload();
            await UnitOfWork.ReloadAsync(Resource, true);
            properties = Resource.Properties.ToArray();
            StateHasChanged();
        }
    }
}
