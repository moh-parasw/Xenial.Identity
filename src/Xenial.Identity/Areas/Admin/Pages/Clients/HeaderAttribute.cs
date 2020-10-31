using System;

namespace Xenial.Identity.Areas.Admin.Pages.Clients
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class HeaderAttribute : Attribute
    {
        public string Header { get; }
        public HeaderAttribute(string header)
            => Header = header;
    }
}
