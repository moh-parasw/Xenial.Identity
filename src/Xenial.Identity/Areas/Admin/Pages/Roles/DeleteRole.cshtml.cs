using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Xenial.Identity.Areas.Admin.Pages.Roles
{
    public class DeleteRoleModel : PageModel
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public DeleteRoleModel(RoleManager<IdentityRole> roleManager)
            => this.roleManager = roleManager;

        public class RoleOutputModel
        {
            public string Name { get; set; }
        }

        public RoleOutputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            if (Input == null)
            {
                var role = await roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    StatusMessage = "Error: Cannot find role";
                    return Page();
                }
                if (role != null)
                {
                    Input = new RoleOutputModel
                    {
                        Name = role.Name
                    };
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string id)
        {
            if (ModelState.IsValid)
            {
                var role = await roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    StatusMessage = "Error: Cannot find role";
                    return Page();
                }
                if (role.Name == "Administrator")
                {
                    StatusMessage = "Cannot delete 'Administrator' role";
                    Input = new RoleOutputModel
                    {
                        Name = role.Name
                    };
                    return Page();
                }
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return Redirect("/Admin/Roles");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Description, error.Description);
                    }
                    StatusMessage = "Error deleting role";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
