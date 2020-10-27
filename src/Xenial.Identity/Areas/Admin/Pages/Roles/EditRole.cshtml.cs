using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Xenial.Identity.Areas.Admin.Pages.Roles
{
    public class EditRoleModel : PageModel
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public EditRoleModel(RoleManager<IdentityRole> roleManager)
            => this.roleManager = roleManager;

        public class RoleInputModel
        {
            [Required]
            public string Name { get; set; }
        }


        [Required, BindProperty]
        public RoleInputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            if (Input == null)
            {
                var role = await roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    StatusMessage = "Cannot find role";
                    return Page();
                }
                if (role != null)
                {
                    Input = new RoleInputModel
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
                    StatusMessage = "Cannot find role";
                    return Page();
                }
                var result = await roleManager.SetRoleNameAsync(role, Input.Name);
                if (result.Succeeded)
                {
                    var updateResult = await roleManager.UpdateAsync(role);

                    if (updateResult.Succeeded)
                    {
                        return Redirect("/Admin/Roles");
                    }
                    else
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(error.Description, error.Description);
                        }
                        StatusMessage = "Error saving role";
                        return Page();
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Description, error.Description);
                    }
                    StatusMessage = "Error setting role name";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
