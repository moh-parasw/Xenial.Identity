using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using Xenial.Identity.Data;
using Xenial.Identity.Models;

namespace Xenial.Identity.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<XenialIdentityUser> signInManager;
        private readonly ILogger<LoginModel> logger;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration configuration;

        public LoginModel(SignInManager<XenialIdentityUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<XenialIdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            UnitOfWork uow)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.emailSender = emailSender;
            this.configuration = configuration;
            var settings = uow.FindObject<XpoApplicationSettings>(null);
            AllowRegister = settings.AllowRegister;
            AllowExternalProviders = settings.AllowExternalProviders;
            AllowGithub = settings.AllowGithub;
        }

        public readonly bool AllowRegister = true;
        public readonly bool AllowExternalProviders = true;
        public readonly bool AllowGithub = true;

        [BindProperty]
        public LoginInputModel LoginInput { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new AuthenticationScheme[0];

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string SelectedPage { get; set; }

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        [BindProperty]
        public RegisterInputModel RegisterInput { get; set; }
        public class RegisterInputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
            public string RegisterConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            SelectedPage = "login";
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!AllowGithub)
            {
                ExternalLogins = ExternalLogins
                    .Where(x => !"github".Equals(x.Name, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostLoginAsync(string returnUrl = null)
        {
            SelectedPage = "login";
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            MarkAllFieldsAsSkipped(nameof(LoginInput));
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await signInManager.PasswordSignInAsync(LoginInput.Email, LoginInput.Password, LoginInput.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, LoginInput.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public void MarkAllFieldsAsSkipped(string name)
        {
            foreach (var state in ModelState.Where(k => !k.Key.StartsWith($"{name}.")).Select(x => x.Value))
            {
                state.Errors.Clear();
                state.ValidationState = ModelValidationState.Skipped;
            }
        }

        public async Task<IActionResult> OnPostRegisterAsync(string returnUrl = null)
        {
            if (!AllowRegister)
            {
                return BadRequest();
            }

            SelectedPage = "register";
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            MarkAllFieldsAsSkipped(nameof(RegisterInput));
            if (ModelState.IsValid)
            {
                //We don't have any users yet. First should be admin.
                const string AdminRole = "Administrator";

                var shouldAddAdminRole = !await userManager.Users.AnyAsync();
                if (shouldAddAdminRole)
                {
                    if (!await roleManager.RoleExistsAsync(AdminRole))
                    {
                        _ = await roleManager.CreateAsync(new IdentityRole(AdminRole));
                        logger.LogInformation($"Created new '{AdminRole}' role.");
                    }
                }

                var user = new XenialIdentityUser { UserName = RegisterInput.Email, Email = RegisterInput.Email };
                var result = await userManager.CreateAsync(user, RegisterInput.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("User created a new account with password.");
                    if (shouldAddAdminRole)
                    {
                        var roleResult = await userManager.AddToRoleAsync(user, AdminRole);
                        if (roleResult.Succeeded)
                        {
                            roleResult = await userManager.UpdateAsync(user);
                            if (roleResult.Succeeded)
                            {
                                logger.LogInformation("Added '{User}' to the '{AdminRole}' role.", user, AdminRole);
                            }
                        }
                    }

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code, returnUrl },
                        protocol: Request.Scheme);

                    await emailSender.SendEmailAsync(RegisterInput.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = RegisterInput.Email, returnUrl });
                    }
                    else
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
