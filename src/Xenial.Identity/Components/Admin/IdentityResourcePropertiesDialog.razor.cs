using System.Security.Claims;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using MudBlazor;

using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Components.Admin;

public partial class IdentityResourcePropertiesDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter, EditorRequired]
    public XpoIdentityResourceProperty Property { get; set; }

    [Parameter]
    public EventCallback<XpoIdentityResourceProperty> PropertyChanged { get; set; }

    [Parameter, EditorRequired]
    public XpoIdentityResource Resource { get; set; }

    [Parameter]
    public bool IsNew { get; set; }

    private void Save()
    {
        Property.Key = key;
        Property.Value = value;
        MudDialog.Close(true);
    }

    private ClaimEditMode Mode { get; set; }

    private string key;
    private string value;

    private void SetKey(string key)
        => this.key = key;

    private void SetValue(string value)
        => this.value = value;

    private void SetMode(ClaimEditMode mode)
    {
        Mode = mode;
        if (mode == ClaimEditMode.Json && string.IsNullOrWhiteSpace(value))
        {
            value = """
            {
                "": ""
            }
            """;
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        value = Property.Value;
        key = Property.Key;
        if (value?.StartsWith("{") ?? false)
        {
            Mode = ClaimEditMode.Json;
        }
        //if (value?.StartsWith("<") ?? false)
        //{
        //    Mode = ClaimEditMode.Xml;
        //}
    }

    private void Cancel()
        => MudDialog?.Close();

    private enum ClaimEditMode
    {
        Text,
        MultiLine,
        Json,
        //Xml
    }
}
