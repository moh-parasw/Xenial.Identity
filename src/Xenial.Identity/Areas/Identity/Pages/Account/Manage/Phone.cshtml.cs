using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Identity.Pages.Account.Manage
{
    public partial class PhoneModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly SignInManager<XenialIdentityUser> signInManager;
        private readonly IEmailSender emailSender;

        public PhoneModel(
            UserManager<XenialIdentityUser> userManager,
            SignInManager<XenialIdentityUser> signInManager,
            //TODO: SMS SENDER https://www.twilio.com/blog/validate-phone-numbers-asp-net-core-identity-razor-pages-lookup
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
        }

        public string Username { get; set; }

        public string Phone { get; set; }

        public bool IsPhoneConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Phone]
            [Display(Name = "Phone number")]
            public string NewPhone { get; set; }
        }

        private async Task LoadAsync(XenialIdentityUser user)
        {
            var phone = await userManager.GetPhoneNumberAsync(user);
            Phone = phone;

            Input = new InputModel
            {
                NewPhone = phone,
            };

            IsPhoneConfirmed = await userManager.IsPhoneNumberConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            //TODO: SMS SENDER https://www.twilio.com/blog/validate-phone-numbers-asp-net-core-identity-razor-pages-lookup

            var result = await userManager.SetPhoneNumberAsync(user, Input.NewPhone);

            //var email = await userManager.GetEmailAsync(user);
            //if (Input.NewPhone != email)
            //{
            //    var userId = await userManager.GetUserIdAsync(user);
            //    var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, Input.NewPhone);
            //    var callbackUrl = Url.Page(
            //        "/Account/ConfirmPhoneChange",
            //        pageHandler: null,
            //        values: new { userId = userId, phone = Input.NewPhone, code = code },
            //        protocol: Request.Scheme);

            //    await emailSender.SendEmailAsync(
            //        Input.NewPhone,
            //        "Confirm your phone",
            //        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            //    StatusMessage = "Confirmation link to change phone number sent. Please check your phones messages.";
            //    return RedirectToPage();
            //}
            StatusMessage = result.Succeeded ? "Your phone number was updated." : "Error updating your phone number.";
            return RedirectToPage();
        }

        //public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        //{
        //    var user = await userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        await LoadAsync(user);
        //        return Page();
        //    }

        //    var userId = await userManager.GetUserIdAsync(user);
        //    var email = await userManager.GetEmailAsync(user);
        //    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //    var callbackUrl = Url.Page(
        //        "/Account/ConfirmPhone",
        //        pageHandler: null,
        //        values: new { area = "Identity", userId = userId, code = code },
        //        protocol: Request.Scheme);
        //    await emailSender.SendEmailAsync(
        //        email,
        //        "Confirm your phone",
        //        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        //    StatusMessage = "Verification message sent. Please check your phones messages.";
        //    return RedirectToPage();
        //}
    }
}
