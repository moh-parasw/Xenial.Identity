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
        ShowFooter = true;
        ShowHearts = true;
        ShowImprint = true;
        ShowLicenses = true;
        ShowCopyright = true;
        ShowTermsOfUse = true;
        ShowRuntimeInformation = true;
        base.AfterConstruction();
    }

    private string customCss = "";
    [Size(SizeAttribute.Unlimited)]
    public string CustomCss { get => customCss; set => SetPropertyValue(nameof(CustomCss), ref customCss, value); }

    private byte[] customLogo = Array.Empty<byte>();
    public byte[] CustomLogo { get => customLogo; set => SetPropertyValue(nameof(CustomLogo), ref customLogo, value); }

    private string customLogoMimeType = "";
    public string CustomLogoMimeType { get => customLogoMimeType; set => SetPropertyValue(nameof(CustomLogoMimeType), ref customLogoMimeType, value); }

    private string logoTeaserFirstRow = "";
    public string LogoTeaserFirstRow { get => logoTeaserFirstRow; set => SetPropertyValue(nameof(LogoTeaserFirstRow), ref logoTeaserFirstRow, value); }

    private string logoTeaserSecondRow = "";
    public string LogoTeaserSecondRow { get => logoTeaserSecondRow; set => SetPropertyValue(nameof(LogoTeaserSecondRow), ref logoTeaserSecondRow, value); }


    private bool showFooter = true;
    public bool ShowFooter { get => showFooter; set => SetPropertyValue(nameof(ShowFooter), ref showFooter, value); }

    private bool showHearts = true;
    public bool ShowHearts { get => showHearts; set => SetPropertyValue(nameof(ShowHearts), ref showHearts, value); }

    private bool showLicenses = true;
    public bool ShowLicenses { get => showLicenses; set => SetPropertyValue(nameof(ShowLicenses), ref showLicenses, value); }

    private bool showImprint = true;
    public bool ShowImprint { get => showImprint; set => SetPropertyValue(nameof(ShowImprint), ref showImprint, value); }

    private bool showTermsOfUse = true;
    public bool ShowTermsOfUse { get => showTermsOfUse; set => SetPropertyValue(nameof(ShowTermsOfUse), ref showTermsOfUse, value); }

    private bool showCopyright = true;
    public bool ShowCopyright { get => showCopyright; set => SetPropertyValue(nameof(ShowCopyright), ref showCopyright, value); }

    private bool showRuntimeInformation = true;
    public bool ShowRuntimeInformation { get => showRuntimeInformation; set => SetPropertyValue(nameof(ShowRuntimeInformation), ref showRuntimeInformation, value); }
}
