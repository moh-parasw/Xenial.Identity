
using System.Collections.Immutable;

using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

using XLocalizer;
using XLocalizer.ErrorMessages;

namespace Xenial.Identity.Infrastructure.Localization;

public sealed class XpoStringLocalizer : IStringLocalizer, IHtmlLocalizer
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

    private ImmutableArray<string> unmatchedLocalizations = ImmutableArray.Create<string>();
    public ImmutableArray<string> UnmatchedLocalizations
    {
        get => unmatchedLocalizations;
        private set
        {
            lock (locker)
            {
                unmatchedLocalizations = value;
            }
        }
    }
    private bool trackUnmatchedLocalizations;
    public bool TrackUnmatchedLocalizations
    {
        get => trackUnmatchedLocalizations; set
        {
            lock (locker)
            {
                trackUnmatchedLocalizations = value;
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
            if (TrackUnmatchedLocalizations)
            {
                UnmatchedLocalizations = UnmatchedLocalizations.Add(name).Distinct().ToImmutableArray();
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
            if (TrackUnmatchedLocalizations)
            {
                UnmatchedLocalizations = UnmatchedLocalizations.Add(name).Distinct().ToImmutableArray();
            }
            return new LocalizedString(name, string.Format(name, arguments));
        }
    }

    LocalizedHtmlString IHtmlLocalizer.this[string name]
    {
        get
        {
            if (LocalizedKeys.TryGetValue(name, out var v))
            {
                return new LocalizedHtmlString(name, v);
            }
            if (TrackUnmatchedLocalizations)
            {
                UnmatchedLocalizations = UnmatchedLocalizations.Add(name).Distinct().ToImmutableArray();
            }
            return new LocalizedHtmlString(name, name);
        }
    }

    LocalizedHtmlString IHtmlLocalizer.this[string name, params object[] arguments]
    {
        get
        {
            if (LocalizedKeys.TryGetValue(name, out var v))
            {
                return new LocalizedHtmlString(name, string.Format(v, arguments));
            }
            if (TrackUnmatchedLocalizations)
            {
                UnmatchedLocalizations = UnmatchedLocalizations.Add(name).Distinct().ToImmutableArray();
            }
            return new LocalizedHtmlString(name, string.Format(name, arguments));
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();

    internal void SetOptions(XLocalizerOptions ops)
    {
        // Data annotation error messages
        ops.ValidationErrors = new ValidationErrors
        {
            CompareAttribute_MustMatch = this["'{0}' and '{1}' do not match."],
            CreditCardAttribute_Invalid = this["The {0} field is not a valid credit card number."],
            CustomValidationAttribute_ValidationError = this["{0} is not valid."],
            DataTypeAttribute_EmptyDataTypeString = this["The custom DataType string cannot be null or empty."],
            EmailAddressAttribute_Invalid = this["The {0} field is not a valid e-mail address."],
            FileExtensionsAttribute_Invalid = this["The {0} field only accepts files with the following extensions: {1}"],
            MaxLengthAttribute_ValidationError = this["The field {0} must be a string or array type with a maximum length of '{1}'."],
            MinLengthAttribute_ValidationError = this["The field {0} must be a string or array type with a minimum length of '{1}'."],
            PhoneAttribute_Invalid = this["The {0} field is not a valid phone number."],
            RangeAttribute_ValidationError = this["The field {0} must be between {1} and {2}."],
            RegexAttribute_ValidationError = this["The field {0} must match the regular expression '{1}'."],
            RequiredAttribute_ValidationError = this["The {0} field is required."],
            StringLengthAttribute_ValidationError = this["The field {0} must be a string with a maximum length of {1}."],
            StringLengthAttribute_ValidationErrorIncludingMinimum = this["The field {0} must be a string with a minimum length of {2} and a maximum length of {1}."],
            UrlAttribute_Invalid = this["The {0} field is not a valid fully-qualified   or ftp URL."],
            ValidationAttribute_ValidationError = this["The field {0} is invalid."],
        };

        // Model binding error messages
        ops.ModelBindingErrors = new ModelBindingErrors
        {
            AttemptedValueIsInvalidAccessor = this["The value '{0}' is not valid for {1}."],
            MissingBindRequiredValueAccessor = this["A value for the '{0}' parameter or property was not provided."],
            MissingKeyOrValueAccessor = this["A value is required."],
            MissingRequestBodyRequiredValueAccessor = this["A non-empty request body is required."],
            NonPropertyAttemptedValueIsInvalidAccessor = this["The value '{0}' is not valid."],
            NonPropertyUnknownValueIsInvalidAccessor = this["The supplied value is invalid."],
            NonPropertyValueMustBeANumberAccessor = this["The field must be a number."],
            UnknownValueIsInvalidAccessor = this["The supplied value is invalid for {0}."],
            ValueIsInvalidAccessor = this["The value '{0}' is invalid."],
            ValueMustBeANumberAccessor = this["The field {0} must be a number."],
            ValueMustNotBeNullAccessor = this["The value '{0}' is invalid."],
        };

        // Identity Errors
        ops.IdentityErrors = new IdentityErrors
        {
            DuplicateEmail = this["Email '{0}' is already taken."],
            DuplicateUserName = this["User name '{0}' is already taken."],
            InvalidEmail = this["Email '{0}' is invalid."],
            DuplicateRoleName = this["Role name '{0}' is already taken."],
            InvalidRoleName = this["Role name '{0}' is invalid."],
            InvalidToken = this["Invalid token."],
            InvalidUserName = this["User name '{0}' is invalid, can only contain letters or digits."],
            LoginAlreadyAssociated = this["A user with this login already exists."],
            PasswordMismatch = this["Incorrect password."],
            PasswordRequiresDigit = this["Passwords must have at least one digit ('0'-'9')."],
            PasswordRequiresLower = this["Passwords must have at least one lowercase ('a'-'z')."],
            PasswordRequiresNonAlphanumeric = this["Passwords must have at least one non alphanumeric character."],
            PasswordRequiresUniqueChars = this["Passwords must use at least {0} different characters."],
            PasswordRequiresUpper = this["Passwords must have at least one uppercase ('A'-'Z')."],
            PasswordTooShort = this["Passwords must be at least {0} characters."],
            UserAlreadyHasPassword = this["User already has a password set."],
            UserAlreadyInRole = this["User already in role '{0}'."],
            UserNotInRole = this["User is not in role '{0}'."],
            UserLockoutNotEnabled = this["Lockout is not enabled for this user."],
            RecoveryCodeRedemptionFailed = this["Recovery code redemption failed."],
            ConcurrencyFailure = this["Optimistic concurrency failure, object has been modified."],
            DefaultError = this["An unknown failure has occurred."],
        };
    }

    public LocalizedString GetString(string name) => throw new NotImplementedException();
    public LocalizedString GetString(string name, params object[] arguments) => throw new NotImplementedException();
    LocalizedString IHtmlLocalizer.GetString(string name) => throw new NotImplementedException();
    LocalizedString IHtmlLocalizer.GetString(string name, params object[] arguments) => throw new NotImplementedException();
    IEnumerable<LocalizedString> IHtmlLocalizer.GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
}
