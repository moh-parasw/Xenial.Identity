using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class ApiResourcesModel : PageModel
    {
        public IQueryable<ApiResourceOutputModel> ApiResources { get; private set; } = new ApiResourceOutputModel[0].AsQueryable();

        public class ApiResourceOutputModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public void OnGet()
        {
        }
    }
}
