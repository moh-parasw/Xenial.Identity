using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class RolesModel : PageModel
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesModel(RoleManager<IdentityRole> roleManager)
            => this.roleManager = roleManager;

        public IQueryable<RoleOutputModel> Roles { get; private set; } = new RoleOutputModel[0].AsQueryable();
        public class RoleOutputModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public async Task OnGet()
            => Roles = (await roleManager.Roles.ToListAsync()).Select(r => new RoleOutputModel
            {
                Id = r.Id,
                Name = r.Name
            }).AsQueryable();

    }
}
