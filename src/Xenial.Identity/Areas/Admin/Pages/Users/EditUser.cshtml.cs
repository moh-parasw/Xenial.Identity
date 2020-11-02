using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages.Users
{
    public class EditUserModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UnitOfWork unitOfWork;

        public EditUserModel(UserManager<XenialIdentityUser> userManager, RoleManager<IdentityRole> roleManager, UnitOfWork unitOfWork)
            => (this.userManager, this.roleManager, this.unitOfWork) = (userManager, roleManager, unitOfWork);

        public class UserInputModel
        {
            [Required]
            public string UserName { get; set; }

            public string Roles { get; set; }
        }

        public string Roles { get; set; }

        public class ClaimModel
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
        }

        public List<ClaimModel> Claims { get; set; } = new List<ClaimModel>();


        [Required, BindProperty]
        public UserInputModel Input { get; set; }

        public string StatusMessage { get; set; }
        public string SelectedPage { get; set; }
        public string Id { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] string selectedPage = null)
        {
            Id = id;
            SelectedPage = selectedPage;
            Roles = string.Join(",", await roleManager.Roles.Select(r => r.Name).ToListAsync());
            if (Input == null)
            {
                var user = await userManager.FindByIdAsync(id);
                var xpoUser = await unitOfWork.GetObjectByKeyAsync<XpoIdentityUser>(id);
                if (user == null || xpoUser == null)
                {
                    StatusMessage = "Error: Cannot find user";
                    return Page();
                }

                Claims = xpoUser.Claims.Select(c => new ClaimModel
                {
                    Id = c.Id,
                    Type = c.Type,
                    Value = c.Value,
                }).ToList();

                var roles = await userManager.GetRolesAsync(user);

                Input = new UserInputModel
                {
                    UserName = user.UserName,
                    Roles = string.Join(",", roles)
                };
            }
            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string id)
        {
            Id = id;
            Roles = string.Join(",", await roleManager.Roles.Select(r => r.Name).ToListAsync());
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    StatusMessage = "Error: Cannot find user";
                    return Page();
                }
                var result = await userManager.SetUserNameAsync(user, Input.UserName);

                if (result.Succeeded)
                {
                    async Task<bool> RemoveFromRoles(IEnumerable<string> roleNames)
                    {
                        foreach (var roleName in roleNames)
                        {
                            var r = await RemoveFromRole(roleName);
                            if (!r)
                            {
                                return false;
                            }
                        }
                        return true;
                    }

                    async Task<bool> RemoveFromRole(string roleName)
                    {
                        if (await userManager.IsInRoleAsync(user, roleName))
                        {
                            var r = await userManager.RemoveFromRoleAsync(user, roleName);
                            return r.Succeeded;
                        }
                        return true;
                    }

                    if (string.IsNullOrEmpty(Input.Roles))
                    {
                        var removeRolesResult = await RemoveFromRoles(roleManager.Roles.Select(r => r.NormalizedName));
                        if (!removeRolesResult)
                        {
                            StatusMessage = "Error updating roles";
                            return Page();
                        }
                    }
                    else
                    {
                        var removeRolesResult = await RemoveFromRoles(roleManager.Roles.Select(r => r.NormalizedName));
                        if (!removeRolesResult)
                        {
                            StatusMessage = "Error updating roles";
                            return Page();
                        }
                        var roleNames = Input.Roles.Split(",").Select(r => roleManager.NormalizeKey(r));
                        try
                        {
                            var rolesResult = await userManager.AddToRolesAsync(user, roleNames);
                            if (!rolesResult.Succeeded)
                            {
                                StatusMessage = "Error updating roles";
                                return Page();
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Roles)}", "Role does not exist");
                            StatusMessage = "Error: Check Validation";
                            return Page();
                        }

                    }

                    var updateResult = await userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        return Redirect("/Admin/Users");
                    }
                    else
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(error.Description, error.Description);
                        }
                        StatusMessage = "Error saving user";
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
