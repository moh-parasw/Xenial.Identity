using System.ComponentModel.DataAnnotations;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Roles
{
    public class EditRoleModel : PageModel
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UnitOfWork unitOfWork;

        public EditRoleModel(RoleManager<IdentityRole> roleManager, UnitOfWork unitOfWork)
            => (this.roleManager, this.unitOfWork) = (roleManager, unitOfWork);

        public class RoleInputModel
        {
            [Required]
            public string Name { get; set; }
        }

        public class ClaimModel
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
        }

        public List<ClaimModel> Claims { get; set; } = new List<ClaimModel>();

        [Required, BindProperty]
        public RoleInputModel Input { get; set; }

        public string StatusMessage { get; set; }
        public string SelectedPage { get; set; }
        public string Id { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] string selectedPage = null)
        {
            Id = id;
            SelectedPage = selectedPage;

            if (Input == null)
            {
                var role = await roleManager.FindByIdAsync(id);
                var xpoRole = await unitOfWork.GetObjectByKeyAsync<XpoIdentityRole>(id);
                if (role == null || xpoRole == null)
                {
                    StatusMessage = "Error: Cannot find role";
                    return Page();
                }

                Input = new RoleInputModel
                {
                    Name = role.Name
                };

                Claims = xpoRole.Claims.Select(c => new ClaimModel
                {
                    Id = c.Id,
                    Type = c.Type,
                    Value = c.Value,
                }).ToList();
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
