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
}
