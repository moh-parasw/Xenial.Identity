using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor.Utilities;

namespace Xenial.Identity.Components;

public partial class MonacoEdit
{
    private string Classes => new CssBuilder("code-editor")
            .AddClass("show-code", Visible)
            .Build();

    private ElementReference el;
    private IJSObjectReference? module;
    private DotNetObjectReference<MonacoEdit>? component;

    [Parameter]
    public string Value { get; set; } = "";

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public CodeLanguage Language { get; set; }

    [Parameter]
    public string Height { get; set; } = "500px";

    [Parameter]
    public string? ExpandHeightOnFocus { get; set; }

    private bool editorFocus;

    private string CalcHeight => editorFocus switch
    {
        true => string.IsNullOrEmpty(ExpandHeightOnFocus) ? Height : ExpandHeightOnFocus,
        false => Height
    };

    [Parameter]
    public EventCallback<string> HeightChanged { get; set; }

    [Parameter]
    public string Width { get; set; } = "100%";

    [Parameter]
    public bool Visible { get; set; } = true;

    [Parameter]
    public bool Formatted { get; set; }

    [Parameter]
    public bool NoLineNumbers { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool Folding { get; set; } = false;

    [Parameter]
    public bool AdjustHeightToContent { get; set; } = false;

    public const string Script = "./_content/Xenial.Identity.Components/js/MonacoEdit/MonacoEdit.js";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", Script);
            component?.Dispose();
            component = DotNetObjectReference.Create(this);
            await CreateEditor();
        }
        else
        {
            if (module is not null && component is not null)
            {
                await module.InvokeVoidAsync("UpdateOptions", component, el);
            }
        }
    }

    private async Task CreateEditor()
    {
        if (module is not null)
        {
            var lang = Language switch
            {
                CodeLanguage.Css => "css",
                CodeLanguage.Html => "html",
                CodeLanguage.Json => "json",
                _ => ""
            };

            var options = new
            {
                lineNumbers = !NoLineNumbers,
                @readonly = ReadOnly,
                formatted = Formatted,
                folding = Folding,
                adjustHeightToContent = AdjustHeightToContent
            };

            await module.InvokeVoidAsync("CreateMonacoEditor", component, el, Value, lang, options);
        }
    }

    [JSInvokable]
    public async Task UpdateValue(string value)
    {
        if (Value != value)
        {
            Value = value;
            await ValueChanged.InvokeAsync(value);
        }
    }

    [JSInvokable]
    public async Task SetHeight(string height)
    {
        Height = height;
        await HeightChanged.InvokeAsync(height);
        StateHasChanged();
    }

    [JSInvokable]
    public void EditorFocus()
    {
        editorFocus = true;
        StateHasChanged();
    }

    [JSInvokable]
    public void EditorBlur()
    {
        editorFocus = false;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (module is { })
            {
                await module.InvokeVoidAsync("DisposeMonacoEditor", el);
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException) { }
        component?.Dispose();
    }

    public async Task Refresh(string newValue)
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("DisposeMonacoEditor", el);
            await UpdateValue(newValue);
            await CreateEditor();
        }
    }
}
