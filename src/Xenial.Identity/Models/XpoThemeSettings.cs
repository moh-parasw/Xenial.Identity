using System;

using DevExpress.Xpo;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Models;

[Persistent("ThemeSettings")]
public class XpoThemeSettings : XpoIdentityBaseObjectString
{
    public XpoThemeSettings(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        Id = Guid.NewGuid().ToString();
        base.AfterConstruction();
    }

    private string customCss = "";
    [Size(SizeAttribute.Unlimited)]
    public string CustomCss { get => customCss; set => SetPropertyValue(nameof(CustomCss), ref customCss, value); }

    private byte[] customLogo = Array.Empty<byte>();
    public byte[] CustomLogo { get => customLogo; set => SetPropertyValue(nameof(CustomLogo), ref customLogo, value); }

    private string customLogoMimeType = "";
    public string CustomLogoMimeType { get => customLogoMimeType; set => SetPropertyValue(nameof(CustomLogoMimeType), ref customLogoMimeType, value); }
}
