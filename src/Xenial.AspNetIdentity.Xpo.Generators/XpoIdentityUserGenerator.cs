using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Xenial.AspNetIdentity.Xpo.Generators
{
    [Generator]
    public class XpoIdentityUserGenerator : XpoIdentityGenerator
    {
        protected override string AttributeText => @"
using System;
namespace Xenial.AspNetIdentity.Xpo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class XPIdentityUserAttribute : Attribute
    {
        public XPIdentityUserAttribute() { }

        public Type RoleType { get; set; }
        public Type ClaimsType { get; set; }
        public Type LoginsType { get; set; }
        public Type TokensType { get; set; }
    }
}
";

        protected override string AttributeFullName => $"Xenial.AspNetIdentity.Xpo.{AttributeName}";

        protected override string AttributeName => "XPIdentityUserAttribute";

        protected override string Name => "XPIdentityUser";

        protected override IEnumerable<(Type type, string name, int size, string[] additionalAttributes)> Fields
        {
            get
            {
                yield return (typeof(string), "UserName", 250, new[] { "Indexed(Unique = true)", "ProtectedPersonalData" });
                yield return (typeof(string), "NormalizedUserName", 250, new[] { "Indexed" });
                yield return (typeof(string), "Email", 250, new[] { "Indexed(Unique = true)", "ProtectedPersonalData" });
                yield return (typeof(string), "NormalizedEmail", 250, new[] { "Indexed" });
                yield return (typeof(bool), "EmailConfirmed", 0, new[] { "PersonalData" });
                yield return (typeof(string), "PasswordHash", 50000, new string[0]);
                yield return (typeof(string), "SecurityStamp", 500, new string[0]);
                yield return (typeof(string), "PhoneNumber", 50, new[] { "ProtectedPersonalData" });
                yield return (typeof(bool), "PhoneNumberConfirmed", 0, new[] { "PersonalData" });
                yield return (typeof(bool), "TwoFactorEnabled", 0, new string[0]);
                yield return (typeof(DateTime?), "LockoutEnd", 0, new string[0]);
                yield return (typeof(bool), "LockoutEnabled", 0, new string[0]);
                yield return (typeof(int), "AccessFailedCount", 0, new string[0]);
            }
        }
        protected override IEnumerable<(string attributeFieldName, string propertyName, bool isAggregated, string[] additionalAttributes)> ManyFields
        {
            get
            {
                yield return ("RoleType", "Roles", false, new string[0]);
                yield return ("ClaimsType", "Claims", true, new string[0]);
                yield return ("LoginsType", "Logins", true, new string[0]);
                yield return ("TokensType", "Tokens", true, new string[0]);
            }
        }
    }
}
