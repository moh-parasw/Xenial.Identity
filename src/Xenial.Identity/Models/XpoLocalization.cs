using DevExpress.Xpo;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Models;

[Persistent("Localization")]
public class XpoLocalization : XpoIdentityBaseObjectString
{
    public XpoLocalization(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        Id = Guid.NewGuid().ToString();
        base.AfterConstruction();
    }

    private string key;
    [Size(1000)]
    public string Key
    {
        get => key;
        set => SetPropertyValue(nameof(Key), ref key, value);
    }

    private string value;
    [Size(1000)]
    public string Value
    {
        get => value;
        set => SetPropertyValue(nameof(Value), ref this.value, value);
    }
}
