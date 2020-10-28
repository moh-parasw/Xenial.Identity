using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources
{
    public class AddApiResourceModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;

        public AddApiResourceModel(UserManager<XenialIdentityUser> userManager)
            => this.userManager = userManager;

        public class ApiResourceInputModel
        {
            [Required]
            public string UserName { get; set; }
        }


        [Required, BindProperty]
        public ApiResourceInputModel Input { get; set; }

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
                    return Redirect("/Admin/ApiResources");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Description, error.Description);
                    }
                    StatusMessage = "Error saving api resource";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
