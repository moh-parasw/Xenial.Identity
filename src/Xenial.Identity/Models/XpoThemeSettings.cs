using DevExpress.Xpo;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Models;

[Persistent("ThemeSettings")]
public class XpoThemeSettings : XpoIdentityBaseObjectString
{
    public XpoThemeSettings(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        Title = "Xenial";
        Id = Guid.NewGuid().ToString();
        ShowFooter = true;
        ShowHearts = true;
        ShowImprint = true;
        ShowCopyright = true;
        ShowTermsOfUse = true;
        ShowRuntimeInformation = true;
        ShowLicenses = true;
        LicenceHtml = @"<li><a href=""/license"">Licenses</a></li>";
        ImprintHtml = @"<li><a href=""/imprint"">Imprint</a></li>";
        TermsOfUseHtml = @"Use of this site constitutes acceptance of our
<a href=""/termsofuse""> Website Terms of Use </a>
and
<a href=""/privacy""> Privacy Policy </a>

<br />";
        CopyrightHtml = @"Copyright © 2012-{{ year }} Manuel Grundner. All
trademarks or registered trademarks are property of their respective
owners.";

        CustomCss = """
            /* .xenial-header__logo__brand-rows {
                display: none;
            }

            .xenial-header__logo img {
                width: 100%;
            } */

            /* Dark Theme */
            /* .xenial__toggletheme-checkbox:not(:checked) ~ * {
                --xenial-styria-green-color: #007e2d;
                --xenial-austria-red-color: #c8102e;
                --xenial-primary-color: #00506a;
                --xenial-secondary-color: #38bcd8;
                --xenial-highlight-color: #f3a027;
                --xenial-lighter-color: #e4e4e4;
                --xenial-light-color: #8b93a1;
                --xenial-dark-color: #667a86;
                --xenial-darker-color: #333333;
                --xenial-white-color: #fff;
                --xenial-black-color: #000;
                --xenial-body-bg-color: #667a86;
                --xenial-nav-bg-color: #667a86;
                --xenial-hamburer-bg-color: rgba($xenial-dark-color, 0.85);
                --xenial-teaser-bg-color: #00506a;
                --xenial-teaser-fg-color: #e4e4e4;
                --xenial-teaser-highlight-color: #38bcd8;
                --xenial-fineprint-bg-color: #38bcd8;
                --xenial-fineprint-fg-color: #e4e4e4;
                --xenial-fineprint-highlight-color: #38bcd8;
                --xenial-tag-color: #f3a027;
                --highlight-fg: #d4d4d4;
                --highlight-bg: #1e1e1e;
                --highlight-selection-bg: #b3d4fc;
                --highlight-token-comment: #6a9955;
                --highlight-token-constant: #b5cea8;
                --highlight-token-string: #ce9178;
                --highlight-token-keyword: #c586c0;
                --highlight-token-function: #dcdcaa;
                --highlight-token-variable: #d16969;
                --highlight-token-parameter: #9cdcfe;
                --highlight-token-classname: #4ec9b0;
                --highlight-token-interpolation: #569cd6;
                --highlight-token-boolean: #569cd6;
                --highlight-token-selector: #d7ba7d;
            } */

            /* Light Theme */
            /* .xenial__toggletheme-checkbox:checked ~ * {
                --xenial-styria-green-color: #007e2d;
                --xenial-austria-red-color: #c8102e;
                --xenial-lighter-color: #333333;
                --xenial-light-color: #667a86;
                --xenial-dark-color: #8b93a1;
                --xenial-darker-color: #e4e4e4;
                --xenial-white-color: #000;
                --xenial-black-color: #fff;
                --xenial-primary-color: #38bcd8;
                --xenial-secondary-color: #00506a;
                --xenial-body-bg-color: #8b93a1;
                --xenial-nav-bg-color: #8b93a1;
                --xenial-hamburer-bg-color: rgba(139, 147, 161, 0.85);
                --xenial-teaser-bg-color: #38bcd8;
                --xenial-teaser-fg-color: #333333;
                --xenial-teaser-highlight-color: #f3a027;
                --xenial-fineprint-bg-color: #38bcd8;
                --xenial-fineprint-fg-color: #e4e4e4;
                --xenial-fineprint-highlight-color: #00506a;
                --xenial-tag-color: #38bcd8;
                --highlight-fg: #393a34;
                --highlight-bg: #ffffff;
                --highlight-selection-bg: #c1def1;
                --highlight-token-comment: #008000;
                --highlight-token-constant: #36acaa;
                --highlight-token-string: #a31515;
                --highlight-token-keyt: aword: #0000ff;
                --highlight-token-function: #393a34;
                --highlight-token-variable: #d16969;
                --highlight-token-parameter: #2b91af;
                --highlight-token-classname: #2b91af;
                --highlight-token-interpolation: #2b91af;
                --highlight-token-boolean: #0000ff;
                --highlight-token-selector: #800000;
            } */
            """;
        base.AfterConstruction();
    }

    private string title;
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    public string Title
    {
        get => title;
        set => SetPropertyValue(nameof(Title), ref title, value);
    }

    private string customCss = "";
    [Size(SizeAttribute.Unlimited)]
    public string CustomCss { get => customCss; set => SetPropertyValue(nameof(CustomCss), ref customCss, value); }

    private byte[] customLogo = Array.Empty<byte>();
    public byte[] CustomLogo { get => customLogo; set => SetPropertyValue(nameof(CustomLogo), ref customLogo, value); }

    private string customLogoMimeType = "";
    public string CustomLogoMimeType { get => customLogoMimeType; set => SetPropertyValue(nameof(CustomLogoMimeType), ref customLogoMimeType, value); }


    private byte[] customFacivon = Array.Empty<byte>();
    public byte[] CustomFacivon
    {
        get => customFacivon;
        set => SetPropertyValue(nameof(CustomFacivon), ref customFacivon, value);
    }

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

    private string licenceHtml = "";
    [Size(SizeAttribute.Unlimited)]
    public string LicenceHtml
    {
        get => licenceHtml;
        set => SetPropertyValue(nameof(LicenceHtml), ref licenceHtml, value);
    }

    private bool showImprint = true;
    public bool ShowImprint { get => showImprint; set => SetPropertyValue(nameof(ShowImprint), ref showImprint, value); }

    private string imprintHtml = "";
    [Size(SizeAttribute.Unlimited)]
    public string ImprintHtml
    {
        get => imprintHtml;
        set => SetPropertyValue(nameof(ImprintHtml), ref imprintHtml, value);
    }

    private bool showTermsOfUse = true;
    public bool ShowTermsOfUse { get => showTermsOfUse; set => SetPropertyValue(nameof(ShowTermsOfUse), ref showTermsOfUse, value); }

    private string termsOfUseHtml = "";
    [Size(SizeAttribute.Unlimited)]
    public string TermsOfUseHtml
    {
        get => termsOfUseHtml;
        set => SetPropertyValue(nameof(TermsOfUseHtml), ref termsOfUseHtml, value);
    }

    private bool showCopyright = true;
    public bool ShowCopyright { get => showCopyright; set => SetPropertyValue(nameof(ShowCopyright), ref showCopyright, value); }

    private string copyrightHtml = "";
    [Size(SizeAttribute.Unlimited)]
    public string CopyrightHtml
    {
        get => copyrightHtml;
        set => SetPropertyValue(nameof(CopyrightHtml), ref copyrightHtml, value);
    }

    private bool showRuntimeInformation = true;
    public bool ShowRuntimeInformation { get => showRuntimeInformation; set => SetPropertyValue(nameof(ShowRuntimeInformation), ref showRuntimeInformation, value); }
}
