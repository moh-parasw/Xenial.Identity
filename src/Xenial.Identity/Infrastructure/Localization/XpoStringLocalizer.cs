using Microsoft.Extensions.Localization;

namespace Xenial.Identity.Infrastructure.Localization;

public class XpoStringLocalizer : IStringLocalizer
{
    public LocalizedString this[string name]
    {
        get
        {
            return new LocalizedString(name, name);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            return new LocalizedString(name, string.Format(name, arguments));
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
}
