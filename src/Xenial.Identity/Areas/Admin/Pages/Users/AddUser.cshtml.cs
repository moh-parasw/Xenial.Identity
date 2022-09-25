using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages.Users
{
    public class AddUserModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;

        public AddUserModel(UserManager<XenialIdentityUser> userManager)
            => this.userManager = userManager;

        public class UserInputModel
        {
            [Required]
            public string UserName { get; set; }
        }


        [Required, BindProperty]
        public UserInputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var role = new XenialIdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = Input.UserName
                };

                var result = await userManager.CreateAsync(role);

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
                    StatusMessage = "Error saving user";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
