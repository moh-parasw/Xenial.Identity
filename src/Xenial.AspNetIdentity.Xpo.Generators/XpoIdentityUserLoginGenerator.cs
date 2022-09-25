﻿using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Xenial.AspNetIdentity.Xpo.Generators
{
    [Generator]
    public class XpoIdentityUserLoginGenerator : XpoIdentityGenerator
    {
        protected override string AttributeText => @"
using System;
namespace Xenial.AspNetIdentity.Xpo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class XPIdentityUserLoginAttribute : Attribute
    {
        public XPIdentityUserLoginAttribute() { }

        public Type UserType { get; set; }
    }
}
";

        protected override string AttributeFullName => $"Xenial.AspNetIdentity.Xpo.{AttributeName}";

        protected override string AttributeName => "XPIdentityUserLoginAttribute";

        protected override string Name => "XPIdentityUserLogin";

        protected override IEnumerable<(Type type, string name, int size, string[] additionalAttributes)> Fields
        {
            get
            {
                yield return (typeof(string), "LoginProvider", 150, new[] { "Indexed(\"User;ProviderKey\", Unique = true)" });
                yield return (typeof(string), "ProviderKey", 150, new string[0]);
                yield return (typeof(string), "ProviderDisplayName", 1000, new string[0]);
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
