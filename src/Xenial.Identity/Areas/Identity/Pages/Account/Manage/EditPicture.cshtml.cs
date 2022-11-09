using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

using IdentityModel;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;
using Xenial.Identity.Infrastructure;

namespace Xenial.Identity.Areas.Identity.Pages.Account.Manage
{
    public class EditPictureModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly ILogger<EditPictureModel> logger;

        public EditPictureModel(
            UserManager<XenialIdentityUser> userManager,
            ILogger<EditPictureModel> logger
        ) => (this.userManager, this.logger) = (userManager, logger);

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

                    if (OperatingSystem.IsWindows())
                    {
                        user.PictureMimeType = GeMimeTypeFromImageByteArray(user.Picture);
                    }

                    if (string.IsNullOrEmpty(user.PictureId)) //We only set a new one if there wasn't one before
                    {
                        user.PictureId = CryptoRandom.CreateUniqueId();
                    }

                    string AbsolutePictureUri(string pictureId)
                    {
                        return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/profile/picture/{pictureId}";
                    }

                    var picture = AbsolutePictureUri(user.PictureId);
                    await userManager.SetOrUpdateClaimAsync(user, new System.Security.Claims.Claim("picture", picture));
                }
                var result = await userManager.UpdateAsync(user);
                StatusMessage = result.Succeeded ? "Profile image updated" : "Error: Profile image upload failed";
            }

            ProfilePicture = new ProfilePictureModel(user);
            return !ModelState.IsValid
                ? isJson ? new JsonResult(ModelState)
                {
                    StatusCode = 400
                } : Page()
                : isJson ? new JsonResult(new
                {
                    StatusMessage,
                    ProfilePicture
                })
                {
                    StatusCode = 200
                } : Page();
        }

        [SupportedOSPlatform("Windows")]
        public string GeMimeTypeFromImageByteArray(byte[] byteArray)
        {
            try
            {
                using var stream = new MemoryStream(byteArray);
                using var image = Image.FromStream(stream);
                return ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == image.RawFormat.Guid).MimeType;
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
            //Don't reset PictureId. Should stay static as long user lives cause tokens are cached
            //user.PictureId = null;
            await userManager.RemoveClaimAsync(user, "picture");

            var result = await userManager.UpdateAsync(user);
            StatusMessage = result.Succeeded ? "Profile image deleted" : "Error: Profile image deletion failed";
            ProfilePicture = new ProfilePictureModel(user);
            ModelState.Clear();
            return RedirectToPage("./EditPicture");
        }
    }
}
