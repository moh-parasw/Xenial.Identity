using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Xenial.Identity.Areas.Admin.Pages.Roles
{
    public class AddRoleModel : PageModel
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public AddRoleModel(RoleManager<IdentityRole> roleManager)
            => this.roleManager = roleManager;

        public class RoleInputModel
        {
            [Required]
            public string Name { get; set; }
        }


        [Required, BindProperty]
        public RoleInputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Input.Name
                };

                var result = await roleManager.CreateAsync(role);

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
                    StatusMessage = "Error saving role";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
