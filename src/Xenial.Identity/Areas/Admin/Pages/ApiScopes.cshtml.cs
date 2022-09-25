using DevExpress.Xpo;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class ApiScopesModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        public ApiScopesModel(UnitOfWork unitOfWork)
            => this.unitOfWork = unitOfWork;


        public IQueryable<ApiScopeOutputModel> ApiScopes { get; set; } = new ApiScopeOutputModel[0].AsQueryable();

        public class ApiScopeOutputModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public async Task OnGet()
            => ApiScopes = (await unitOfWork.Query<XpoApiScope>().Select(r => new ApiScopeOutputModel
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync()).AsQueryable();
    }
}
