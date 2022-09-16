using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace FairWebBlazor.Components;

public partial class BoolSwitch
{
    /// <summary>
    /// The value the user selects
    /// </summary>
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// The label of the bool value
    /// </summary>
    [Parameter]
    public string Label { get; set; } = "";

    /// <summary>
    /// Occures when the value changes
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    private Variant? variant;
    /// <summary>
    /// Variant can be Text, Filled or Outlined.
    /// Defaults to <see cref="DefaultVariant"/>.
    /// </summary>
    [Parameter]
    public Variant Variant
    {
        //We use the parameter, if it's null 
        //we use the CascadingVariant, if it's null 
        //we use the DefaultVariant
        get => variant switch
        {
            { } v => v,
            _ => CascadingVariant switch
            {
                { } v2 => v2,
                _ => Variant.Filled
            }
        }; set => variant = value;
    }

    [CascadingParameter]
    public Variant? CascadingVariant { get; set; }

    /// <summary>
    /// The color of the component. It supports the theme colors.
    /// Defaults to Color.Secondary
    /// </summary>
    [Parameter]
    public Color Color { get; set; } = Color.Secondary;

    /// <summary>
    /// User class names, separated by space.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.ComponentBase.Common)]
    public string Class { get; set; } = "";

    /// <summary>
    /// User styles, applied on top of the component's own classes and styles.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.ComponentBase.Common)]
    public string Style { get; set; } = "";

    /// <summary>
    /// Use Tag to attach any user data object to the component for your convenience.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.ComponentBase.Common)]
    public object Tag { get; set; } = null!;

    /// <summary>
    /// UserAttributes carries all attributes you add to the component that don't match any of its parameters.
    /// They will be splatted onto the underlying HTML tag.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [Category(CategoryTypes.ComponentBase.Common)]
    public Dictionary<string, object> UserAttributes { get; set; } = new Dictionary<string, object>();

    [Parameter]
    public string LabelForTrue { get; set; } = "";

    [Parameter]
    public string LabelForFalse { get; set; } = "";

    [Parameter]
    public string IconForTrue { get; set; } = "";

    [Parameter]
    public string IconForFalse { get; set; } = "";

    [Parameter]
    public Color ColorForTrue { get; set; } = Color.Default;

    [Parameter]
    public Color ColorForFalse { get; set; } = Color.Default;

    //We switch the labels because it should indicate the
    //target state to the user.
    //That's better from a Usability standpoint
    private string SwitchLabel => Value
        ? LabelForFalse
        : LabelForTrue;

    private string IconLabel => Value
        ? IconForTrue
        : IconForFalse;


    private Color IconColor => Value
        ? ColorForTrue
        : ColorForFalse;
}
