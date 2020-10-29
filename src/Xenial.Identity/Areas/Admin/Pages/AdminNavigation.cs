using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public static class AdminNavigation
    {
        public static string Index => "Index";

        public static string Clients => "Clients";

        public static string IdentityResources => "IdentityResources";

        public static string ApiResources => "ApiResources";

        public static string ApiScopes => "ApiScopes";

        public static string PersistedGrants => "PersistedGrants";

        public static string Users => "Users";

        public static string Roles => "Roles";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string ClientsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Clients);

        public static string IdentityResourcesNavClass(ViewContext viewContext) => PageNavClass(viewContext, IdentityResources);

        public static string ApiResourcesNavClass(ViewContext viewContext) => PageNavClass(viewContext, ApiResources);
        public static string ApiScopesNavClass(ViewContext viewContext) => PageNavClass(viewContext, ApiScopes);

        public static string PersistedGrantsNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersistedGrants);

        public static string UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);

        public static string RolesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Roles);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
