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
        private readonly UnitOfWork unitOfWork;

        public EditUserModel(UserManager<XenialIdentityUser> userManager, UnitOfWork unitOfWork)
            => (this.userManager, this.unitOfWork) = (userManager, unitOfWork);

        public class UserInputModel
        {
            [Required]
            public string UserName { get; set; }


        }
        public class ClaimModel
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public string Issuer { get; set; }
        }

        public List<ClaimModel> Claims { get; set; } = new List<ClaimModel>();


        [Required, BindProperty]
        public UserInputModel Input { get; set; }

        public string StatusMessage { get; set; }
        public string SelectedPage { get; set; }
        public string Id { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            Id = id;
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

                Input = new UserInputModel
                {
                    UserName = user.UserName,
                };
            }
            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string id)
        {
            Id = id;
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
