using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;

using Xenial.Identity.Components;
using Xenial.Identity.Data;
using Xenial.Identity.Infrastructure;

namespace Xenial.Identity.Areas.Identity.Pages.Account.Manage
{
    public class EditProfileModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public class InputModel
        {
            [Display(Name = "Full Name")]
            public string FullName { get; set; }
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [BindProperty(SupportsGet = true)]
            public string Color { get; set; }
            public string Initials { get; set; }


            [Display(Name = "Company Name")]
            public string CompanyName { get; set; }

            [Display(Name = "Address 1")]
            public string AddressStreetAddress1 { get; set; }

            [Display(Name = "Address 2")]
            public string AddressStreetAddress2 { get; set; }

            [Display(Name = "City")]
            public string AddressLocality { get; set; }

            [Display(Name = "State or Province")]
            public string AddressRegion { get; set; }

            [Display(Name = "Zipcode or Postal Code")]
            public string AddressPostalCode { get; set; }
            [Display(Name = "County")]
            public string AddressCountry { get; set; }
        }

        public string StatusMessage { get; set; }

        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly SignInManager<XenialIdentityUser> signInManager;
        private readonly ILogger<EditProfileModel> logger;

        public EditProfileModel(
            UserManager<XenialIdentityUser> userManager,
            SignInManager<XenialIdentityUser> signInManager,
            ILogger<EditProfileModel> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        public async Task OnGet()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return;
            }
            var mapper = mapperConfiguration.CreateMapper();
            Input = mapper.Map(user, Input);
        }

        private class ModelMapperProfile : Profile
        {
            public ModelMapperProfile()
                => CreateMap<InputModel, XenialIdentityUser>().ReverseMap();
        }

        private static readonly AutoMapper.IConfigurationProvider mapperConfiguration = new MapperConfiguration(o => o.AddProfile<ModelMapperProfile>());

        public async Task OnPost()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("No user found {User}", User);
                return;
            }
            if (ModelState.IsValid)
            {
                var mapper = mapperConfiguration.CreateMapper();
                user = mapper.Map(Input, user);
                user.UpdatedAt = DateTime.Now;

                await userManager.SetOrUpdateClaimAsync(user, new Claim("name", user.FullName ?? string.Empty));
                await userManager.SetOrUpdateClaimAsync(user, new Claim("family_name", user.LastName ?? string.Empty));
                await userManager.SetOrUpdateClaimAsync(user, new Claim("given_name", user.FirstName ?? string.Empty));
                //await SetOrUpdateClaimAsync(user, new Claim("website", user.Website ?? string.Empty));
                //await SetOrUpdateClaimAsync(user, new Claim("profile", absProfileUrl));

                //TODO: Map if needed
                //await SetOrUpdateClaimAsync(user, new Claim("gender", user.Gender ?? string.Empty));
                //await SetOrUpdateClaimAsync(user, new Claim("birthdate", user.Birthdate?.ToString("YYYY-MM-DD") ?? string.Empty));
                //await SetOrUpdateClaimAsync(user, new Claim("zoneinfo", user.Zoneinfo ?? string.Empty));
                //await SetOrUpdateClaimAsync(user, new Claim("locale", user.Locale ?? string.Empty));
                await userManager.SetOrUpdateClaimAsync(user, new Claim("updated_at", ConvertToUnixTimestamp(user.UpdatedAt)?.ToString() ?? string.Empty));
                await userManager.SetOrUpdateClaimAsync(user, new Claim("xenial_backcolor", user.Color ?? string.Empty));
                await userManager.SetOrUpdateClaimAsync(user, new Claim("xenial_forecolor", (MaterialColorPicker.ColorIsDark(user.Color) ? "#FFFFFF" : "#000000") ?? string.Empty));
                await userManager.SetOrUpdateClaimAsync(user, new Claim("xenial_initials", user.Initials ?? string.Empty));

                var streetAddress = string.Join(" ", new[] { user.AddressStreetAddress1, user.AddressStreetAddress2 }.Where(s => !string.IsNullOrWhiteSpace(s)));
                var postalAddress = string.Join(" ", new[] { user.AddressPostalCode, user.AddressLocality }.Where(s => !string.IsNullOrWhiteSpace(s)));

                await userManager.SetOrUpdateClaimAsync(user, new Claim("address", JsonConvert.SerializeObject(new
                {
                    formatted = string.Join(Environment.NewLine, new[] { streetAddress, postalAddress, user.AddressRegion, user.AddressCountry }.Where(s => !string.IsNullOrWhiteSpace(s))) ?? string.Empty,
                    street_address = streetAddress ?? string.Empty,
                    locality = user.AddressLocality ?? string.Empty,
                    region = user.AddressRegion ?? string.Empty,
                    postal_code = user.AddressPostalCode ?? string.Empty,
                    country = user.AddressCountry ?? string.Empty,
                }, Formatting.Indented)));

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Error updating user";
                    ModelState.AddModelError("User", "Error updating user");
                }
                else
                {
                    StatusMessage = "Profile was updated successfully.";
                    await signInManager.RefreshSignInAsync(user);
                }
            }
            else
            {
                StatusMessage = "Error: Validation failed. Please check your inputs.";
            }
        }

        private static double? ConvertToUnixTimestamp(DateTime? date)
        {
            if (date.HasValue)
            {
                var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var diff = date.Value.ToUniversalTime() - origin;
                return Math.Floor(diff.TotalSeconds);
            }
            return null;
        }
    }
}
