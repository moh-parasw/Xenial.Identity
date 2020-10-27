using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevExpress.Pdf.Native.BouncyCastle.Asn1.Cms;
using DevExpress.Xpo;
using DevExpress.XtraPrinting.Export.Pdf;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class RolesModel : PageModel
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesModel(RoleManager<IdentityRole> roleManager)
            => this.roleManager = roleManager;

        public IList<RoleOutputModel> Roles { get; private set; } = new RoleOutputModel[0];
        public class RoleOutputModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public async Task OnGet(CancellationToken cancellationToken = default)
            => Roles = (await roleManager.Roles.ToListAsync(cancellationToken)).Select(r => new RoleOutputModel
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

    }
}
