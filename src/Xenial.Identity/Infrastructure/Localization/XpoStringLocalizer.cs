using System.Collections.Immutable;

using Microsoft.Extensions.Localization;

using XLocalizer;
using XLocalizer.ErrorMessages;

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

    internal void SetOptions(XLocalizerOptions ops)
    {
        // Data annotation error messages
        ops.ValidationErrors = new ValidationErrors
        {
            RequiredAttribute_ValidationError = this["The {0} field is required."],
            CompareAttribute_MustMatch = "'{0}' and '{1}' do not match.",
            StringLengthAttribute_ValidationError = "The field {0} must be a string with a maximum length of {1}.",
            // ...
        };

        // Model binding error messages
        ops.ModelBindingErrors = new ModelBindingErrors
        {
            AttemptedValueIsInvalidAccessor = "The value '{0}' is not valid for {1}.",
            MissingBindRequiredValueAccessor = "A value for the '{0}' parameter or property was not provided.",
            // ...
        };

        // Identity Errors
        ops.IdentityErrors = new IdentityErrors
        {
            DuplicateEmail = "Email '{0}' is already taken.",
            DuplicateUserName = "User name '{0}' is already taken.",
            // ...
        };
    }
}
