using System;

using DevExpress.Xpo;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Models;

[Persistent("ApplicationSettings")]
public class XpoApplicationSettings : XpoIdentityBaseObjectString
{
    public XpoApplicationSettings(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        Id = Guid.NewGuid().ToString();
        AllowRegister = true;
        AllowExternalProviders = true;
        AllowGithub = true;
        AllowOTP = true;
        enableSMSOTP = true;
        EnableEmailOTP = true;
        base.AfterConstruction();
    }

    private bool allowRegister = true;
    public bool AllowRegister
    {
        get => allowRegister;
        set => SetPropertyValue(nameof(AllowRegister), ref allowRegister, value);
    }

    private bool allowExternalProviders = true;
    public bool AllowExternalProviders
    {
        get => allowExternalProviders;
        set => SetPropertyValue(nameof(AllowExternalProviders), ref allowExternalProviders, value);
    }

    private bool allowGithub = true;
    public bool AllowGithub
    {
        get => allowGithub;
        set => SetPropertyValue(nameof(AllowGithub), ref allowGithub, value);
    }

    private bool allowOTP = true;
    public bool AllowOTP
    {
        get => allowOTP;
        set => SetPropertyValue(nameof(AllowOTP), ref allowOTP, value);
    }

    private bool enableSMSOTP = true;
    public bool EnableSMSOTP
    {
        get => enableSMSOTP;
        set => SetPropertyValue(nameof(EnableSMSOTP), ref enableSMSOTP, value);
    }

    private bool enableEmailOTP = true;
    public bool EnableEmailOTP
    {
        get => enableEmailOTP;
        set => SetPropertyValue(nameof(EnableEmailOTP), ref enableEmailOTP, value);
    }
}
