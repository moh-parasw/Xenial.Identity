using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Xenial.Identity.Components;

public partial class XTerm
{
    public const string Script = "./_content/Xenial.Identity.Components/js/XTerm/XTerm.js";

    private IJSObjectReference? module;
    private DotNetObjectReference<XTerm>? component;

    [Parameter]
    public EventCallback Initialized { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", Script);
            component?.Dispose();
            component = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("CreateTerminal", el);
            await Initialized.InvokeAsync();
        }
    }

    public async Task Write(string buffer)
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("WriteTerminal", el, buffer);
        }
    }

    public async Task Write(byte[] buffer)
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("WriteTerminal", el, buffer);
        }
    }

    public async Task WriteLine(string buffer)
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("WriteLineTerminal", el, buffer);
        }
    }

    public async Task WriteLine(byte[] buffer)
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("WriteLineTerminal", el, buffer);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (module is { })
            {
                await module.InvokeVoidAsync("DisposeTerminal", el);
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException) { }
        component?.Dispose();
    }
}
