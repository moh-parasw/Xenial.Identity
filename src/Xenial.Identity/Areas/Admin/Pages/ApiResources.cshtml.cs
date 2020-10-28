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
    public class ApiResourcesModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        public ApiResourcesModel(UnitOfWork unitOfWork)
            => this.unitOfWork = unitOfWork;


        public IQueryable<ApiResourceOutputModel> ApiResources { get; set; } = new ApiResourceOutputModel[0].AsQueryable();

        public class ApiResourceOutputModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public async Task OnGet()
            => ApiResources = (await unitOfWork.Query<XpoApiResource>().Select(r => new ApiResourceOutputModel
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync()).AsQueryable();
    }
}
