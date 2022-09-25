using DevExpress.Xpo;

using MudBlazor;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class IdentityResourcePropertiesSmall
{
    private async Task Edit(XpoIdentityResourceProperty property = null)
    {
        using var uow = UnitOfWork.BeginNestedUnitOfWork();
        var resource = uow.GetObjectByKey<XpoIdentityResource>(Resource.Id);
        var currentProperty = property switch
        {
            { } => uow.GetObjectByKey<XpoIdentityResourceProperty>(property.Id),
            _ => new XpoIdentityResourceProperty(uow) { IdentityResource = resource }
        };
        var dialog = DialogService.Show<IdentityResourcePropertiesDialog>(
            property == null ? "Add Property" : "Edit Property",
            new DialogParameters
            {
                [nameof(IdentityResourcePropertiesDialog.Property)] = currentProperty,
                [nameof(IdentityResourcePropertiesDialog.IsNew)] = property == null,
                [nameof(IdentityResourcePropertiesDialog.Resource)] = resource,
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

    private async Task Delete(XpoIdentityResourceProperty property)
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

                _ = Snackbar.Add($"""
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

                _ = Snackbar.Add($"""
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
