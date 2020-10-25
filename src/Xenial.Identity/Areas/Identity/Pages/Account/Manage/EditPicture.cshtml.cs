using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using DevExpress.XtraReports.Design;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

        public ProfilePictureModel ProfilePicture { get; set; }

        public async Task OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return;
            }

            ProfilePicture = new ProfilePictureModel(user);
        }

        public string StatusMessage { get; set; }

        [BindProperty, Required]
        public IFormFile Upload { get; set; }

        public async Task<ActionResult> OnPostUploadAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return Page();
            }

            ProfilePicture = new ProfilePictureModel(user);
            var isJson = Request.GetTypedHeaders().Accept.Contains(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

            if (Upload == null)
            {
                StatusMessage = "Error: Upload is empty. Please select a picture.";
                return isJson ? new JsonResult(new
                {
                    StatusMessage
                })
                {
                    StatusCode = 400
                } : Page();
            }

            //TODO: CHECK FOR IMAGE TYPE
            if (ModelState.IsValid)
            {
                using (var stream = new MemoryStream())
                {
                    await Upload.CopyToAsync(stream);
                    stream.Position = 0;
                    user.Picture = stream.ToArray();
                    user.PictureMimeType = GeMimeTypeFromImageByteArray(user.Picture);
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

            ProfilePicture = new ProfilePictureModel(user);
            if (!ModelState.IsValid)
            {
                return isJson ? new JsonResult(ModelState)
                {
                    StatusCode = 400
                } : Page();
            }

            return isJson ? new JsonResult(new
            {
                StatusMessage,
                ProfilePicture
            })
            {
                StatusCode = 200
            } : Page();
        }

        public string GeMimeTypeFromImageByteArray(byte[] byteArray)
        {
            try
            {
                using (var stream = new MemoryStream(byteArray))
                using (var image = Image.FromStream(stream))
                {
                    return ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == image.RawFormat.Guid).MimeType;
                }
            }
            catch (Exception ex)
            {
                var svgMimeType = "image/svg+xml";
                logger.LogError(ex, $"Cannot infer image mime type, set to {svgMimeType} {{Message}}", ex.Message);
                return svgMimeType;
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return Page();
            }
            user.Picture = new byte[0];
            user.PictureMimeType = null;
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                StatusMessage = "Profile image deleted";
            }
            else
            {
                StatusMessage = "Error: Profile image deletion failed";
            }
            ProfilePicture = new ProfilePictureModel(user);
            ModelState.Clear();
            return RedirectToPage("./EditPicture");
        }
    }
}
