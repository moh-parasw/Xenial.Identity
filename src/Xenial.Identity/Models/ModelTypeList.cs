namespace Xenial.Identity.Models;

public static class XenialIdentityModelTypeList
{
    public static readonly Type[] ModelTypes = new[]
    {
        typeof(XpoXeniaIIdentityUser),
        typeof(XpoThemeSettings),
        typeof(XpoApplicationSettings),
        typeof(XpoLocalization),
        typeof(XpoCommunicationChannel)
    };
}
