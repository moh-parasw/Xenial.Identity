using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Identity.Pages.Account.Manage
{
    public class EditPictureModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly ILogger<EditPictureModel> logger;
        public EditPictureModel(
            UserManager<XenialIdentityUser> userManager,
            ILogger<EditPictureModel> logger
        )
            => (this.userManager, this.logger) = (userManager, logger);

        public async Task OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return;
            }

            ImageUri = CreateImageUri(user);
        }

        private static string CreateImageUri(XenialIdentityUser user)
        {
            if (user.Picture != null && user.Picture.Length > 0)
            {
                return $"data:image/png;base64,{Convert.ToBase64String(user.Picture)}";
            }
            return null;
        }

        public string ImageUri { get; set; }
        public string StatusMessage { get; set; }

        [BindProperty, Required]
        public IFormFile Upload { get; set; }

        public async Task OnPostUploadAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return;
            }

            ImageUri = CreateImageUri(user);

            if (Upload == null)
            {
                StatusMessage = "Error: Upload is empty. Please select a picture.";
                return;
            }

            if (ModelState.IsValid)
            {
                using (var stream = new MemoryStream())
                {
                    await Upload.CopyToAsync(stream);
                    stream.Position = 0;
                    user.Picture = stream.ToArray();
                }
                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    StatusMessage = "Profile image updated";
                }
                else
                {
                    StatusMessage = "Error: Profile image upload failed";
                }
            }
            ImageUri = CreateImageUri(user);
        }

        public async Task OnPostDeleteAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return;
            }
            user.Picture = new byte[0];
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                StatusMessage = "Profile image deleted";
            }
            else
            {
                StatusMessage = "Error: Profile image deletion failed";
            }
            ImageUri = CreateImageUri(user);
        }
    }
}
