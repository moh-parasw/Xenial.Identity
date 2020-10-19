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
    public class XpoIdentityUserClaimGenerator : XpoIdentityGenerator
    {
        protected override string AttributeText => @"
using System;
namespace Xenial.AspNetIdentity.Xpo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class XPIdentityUserClaimAttribute : Attribute
    {
        public XPIdentityUserClaimAttribute() { }

        public Type UserType { get; set; }
        public Type ClaimType { get; set; }
    }
}
";

        protected override string AttributeFullName => $"Xenial.AspNetIdentity.Xpo.{AttributeName}";

        protected override string AttributeName => "XPIdentityUserClaimAttribute";

        protected override string Name => "XPIdentityUserClaim";

        protected override IEnumerable<(Type type, string name, int size, string[] additionalAttributes)> Fields
        {
            get
            {
                yield return (typeof(string), "Type", 250, new string[0]);
                yield return (typeof(string), "Value", 250, new string[0]);
            }
        }
        protected override IEnumerable<(string attributeFieldName, string propertyName, bool isAggregated, string[] additionalAttributes)> OneFields
        {
            get
            {
                yield return ("RoleType", "Roles", false, new string[0]);
                yield return ("ClaimType", "Claimes", true, new string[0]);
            }
        }
    }
}
