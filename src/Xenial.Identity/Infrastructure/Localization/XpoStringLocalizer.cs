using System.Collections.Immutable;

using Microsoft.Extensions.Localization;

namespace Xenial.Identity.Infrastructure.Localization;

public sealed class XpoStringLocalizer : IStringLocalizer
{
    private static readonly object locker = new();

    private ImmutableDictionary<string, string> localizedKeys = ImmutableDictionary.Create<string, string>();
    public ImmutableDictionary<string, string> LocalizedKeys
    {
        get => localizedKeys;
        private set
        {
            lock (locker)
            {
                localizedKeys = value;
            }
        }
    }

    public void UpdateDictionary(IReadOnlyDictionary<string, string> newValues)
        => UpdateDictionary(newValues.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));

    public void UpdateDictionary(IEnumerable<KeyValuePair<string, string>> newValues)
        => LocalizedKeys = LocalizedKeys.Clear().AddRange(newValues);

    public LocalizedString this[string name]
    {
        get
        {
            if (LocalizedKeys.TryGetValue(name, out var v))
            {
                return new LocalizedString(name, v);
            }
            return new LocalizedString(name, name);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            if (LocalizedKeys.TryGetValue(name, out var v))
            {
                return new LocalizedString(name, string.Format(v, arguments));
            }
            return new LocalizedString(name, string.Format(name, arguments));
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
}
