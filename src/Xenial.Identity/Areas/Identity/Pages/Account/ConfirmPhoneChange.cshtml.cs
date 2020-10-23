using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmPhoneChangeModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly SignInManager<XenialIdentityUser> signInManager;

        public ConfirmPhoneChangeModel(UserManager<XenialIdentityUser> userManager, SignInManager<XenialIdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string phone, string code)
        {
            if (userId == null || phone == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await userManager.ChangePhoneNumberAsync(user, phone, code);
            if (!result.Succeeded)
            {
                StatusMessage = "Error changing phone number.";
                return Page();
            }

            await signInManager.RefreshSignInAsync(user);
            StatusMessage = "Thank you for confirming your phone number change.";
            return Page();
        }
    }
}
