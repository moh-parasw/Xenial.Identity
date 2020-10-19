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
    public class XpoIdentityUserTokenGenerator : XpoIdentityGenerator
    {
        protected override string AttributeText => @"
using System;
namespace Xenial.AspNetIdentity.Xpo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class XPIdentityUserTokenAttribute : Attribute
    {
        public XPIdentityUserTokenAttribute() { }

        public Type UserType { get; set; }
    }
}
";

        protected override string AttributeFullName => $"Xenial.AspNetIdentity.Xpo.{AttributeName}";

        protected override string AttributeName => "XPIdentityUserTokenAttribute";

        protected override string Name => "XPIdentityUserToken";

        protected override IEnumerable<(Type type, string name, int size, string[] additionalAttributes)> Fields
        {
            get
            {
                yield return (typeof(string), "LoginProvider", 150, new[] { "Indexed(\"User;Name\", Unique = true)" });
                yield return (typeof(string), "Name", 150, new string[0]);
                yield return (typeof(string), "Value", 1000, new string[0]);
            }
        }
        protected override IEnumerable<(string attributeFieldName, string propertyName, bool isAggregated, string[] additionalAttributes)> OneFields
        {
            get
            {
                yield return ("UserType", "User", false, new string[0]);
            }
        }
    }
}
