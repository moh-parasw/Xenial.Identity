
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Xenial.AspNetIdentity.Xpo.Generators
{
    [Generator]
    public class XpoIdentityRoleGenerator : XpoIdentityGenerator
    {
        protected override string AttributeText => @"using System;
namespace Xenial.AspNetIdentity.Xpo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class XPIdentityRoleAttribute : Attribute
    {
        public XPIdentityRoleAttribute() { }

        public Type UserType { get; set; }
        public Type ClaimsType { get; set; }
    }
}
";

        protected override string AttributeFullName => "Xenial.AspNetIdentity.Xpo.XPIdentityRoleAttribute";
        protected override string AttributeName => "XPIdentityRoleAttribute";
        protected override string Name => "XPIdentityRole";

        protected override IEnumerable<(Type type, string name, int size, string[] additionalAttributes)> Fields
        {
            get
            {
                yield return (typeof(string), "Name", 50, new[] { "Indexed(Unique = true)" });
                yield return (typeof(string), "NormalizedName", 50, new[] { "Indexed" });
            }
        }

        protected override IEnumerable<(string attributeFieldName, string propertyName, bool isAggregated, string[] additionalAttributes)> ManyFields
        {
            get
            {
                yield return ("UserType", "Users", false, new string[0]);
            }
        }
    }
}
