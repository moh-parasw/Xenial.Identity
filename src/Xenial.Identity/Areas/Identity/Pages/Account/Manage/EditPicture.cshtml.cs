using System;
using System.Collections.Generic;
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
            if (user.Picture != null && user.Picture.Length > 0)
            {
                ImageUri = CreateImageUri(user);
            }
            else
            {
                ImageUri = null;
            }
        }

        private static string CreateImageUri(XenialIdentityUser user) => $"data:image/png;base64,{Convert.ToBase64String(user.Picture)}";
        public string ImageUri { get; set; }
        public string StatusMessage { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return;
            }
            if (Upload == null)
            {
                StatusMessage = "Error: Upload is null";
                return;
            }

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
                ImageUri = CreateImageUri(user);
            }
            else
            {
                StatusMessage = "Error: Profile image upload failed";
            }
        }
    }
}
