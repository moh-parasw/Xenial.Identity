using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages.Users
{
    public class DeleteUserModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;

        public DeleteUserModel(UserManager<XenialIdentityUser> userManager)
            => this.userManager = (userManager);

        public class UserOutputModel
        {
            public string UserName { get; set; }
        }

        public UserOutputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            if (Input == null)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    StatusMessage = "Cannot find role";
                    return Page();
                }
                if (user != null)
                {
                    Input = new UserOutputModel
                    {
                        UserName = user.UserName
                    };
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string id)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(id);
                var currentUser = await userManager.GetUserAsync(User);
                if (user == null || currentUser == null)
                {
                    StatusMessage = "Error: Cannot find user";
                    return Page();
                }

                if (user.Id == currentUser.Id)
                {
                    StatusMessage = "Error: Cannot delete current user";
                    Input = new UserOutputModel
                    {
                        UserName = user.UserName
                    };
                    return Page();
                }
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Redirect("/Admin/Users");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Description, error.Description);
                    }
                    StatusMessage = "Error deleting user";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
