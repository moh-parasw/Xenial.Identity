using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using MudBlazor;
using MudBlazor.Utilities;

using Spectre.Console;

namespace Xenial.Identity;

public partial class TagChipsEdit
{
    public string Value { get; set; } = "";

    [Parameter]
    public string Label { get; set; } = "";

    [Parameter]
    public bool ClearAfterAdd { get; set; } = true;

    [Parameter]
    public IEnumerable<string> Items { get; set; } = Enumerable.Empty<string>();

    [Parameter]
    public EventCallback<IEnumerable<string>> ItemsChanged { get; set; }

    public async Task KeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrEmpty(Value))
        {
            await AddItem();
        }
    }

    public Task AddItem()
        => AddItem(Value);

    public async Task AddItem(string item)
    {
        if (!Items.Contains(item) && !string.IsNullOrEmpty(item))
        {
            if (ClearAfterAdd)
            {
                Value = "";
            }
            Items = Items.Concat(new[] { item });
            await ItemsChanged.InvokeAsync(Items);
        }
    }

    private Task RemoveItem(MudChip chip)
        => RemoveItem(chip.Text);

    public async Task RemoveItem(string item)
    {
        Items = Items.Concat(new[] { item });
        await ItemsChanged.InvokeAsync(Items);
    }

}
