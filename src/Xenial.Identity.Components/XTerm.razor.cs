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

    [Parameter]
    public int? Columns { get; set; }

    [Parameter]
    public int? Rows { get; set; }

    [Parameter]
    public bool Autofit { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", Script);
            component?.Dispose();
            component = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("CreateTerminal", el, Columns, Rows, Autofit);
            await Initialized.InvokeAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (module is not null)
        {
            await module.InvokeVoidAsync("ResizeTerminal", el, Columns, Rows);
        }
    }

    public async Task ScrollToBottom()
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("ScrollTerminalToBottom", el);
        }
    }

    public async Task Fit()
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("FitTerminal", el);
        }
    }

    public async Task Clear()
    {
        if (module is { })
        {
            await module.InvokeVoidAsync("ClearTerminal", el);
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
