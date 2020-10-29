using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class IdentityResourcesModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        public IdentityResourcesModel(UnitOfWork unitOfWork)
            => this.unitOfWork = unitOfWork;

        public IQueryable<IdentityResourceOutputModel> IdentityResources { get; set; } = new IdentityResourceOutputModel[0].AsQueryable();

        public class IdentityResourceOutputModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public async Task OnGet()
            => IdentityResources = (await unitOfWork.Query<XpoIdentityResource>().Select(r => new IdentityResourceOutputModel
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync()).AsQueryable();
    }
}
