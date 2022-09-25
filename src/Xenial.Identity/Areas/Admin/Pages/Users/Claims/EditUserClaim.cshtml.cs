using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Users.Claims
{
    public class EditUserClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditUserClaimModel> logger;
        public EditUserClaimModel(UnitOfWork unitOfWork, ILogger<EditUserClaimModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class UserClaimInputModel
        {
            [Required]
            public string Type { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class UserMappingConfiguration : Profile
        {
            public UserMappingConfiguration()
                => CreateMap<UserClaimInputModel, XpoIdentityUserClaim>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<UserMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public UserClaimInputModel Input { get; set; } = new UserClaimInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string userId, [FromRoute] string id)
        {
            var user = await unitOfWork.GetObjectByKeyAsync<XpoIdentityUser>(userId);
            if (user == null)
            {
                StatusMessage = "Error: cannot find user";
                return Page();
            }

            var apiResourceClaim = user.Claims.FirstOrDefault(user => user.Id == id);
            if (apiResourceClaim == null)
            {
                StatusMessage = "Error: cannot find user claim";
                return Page();
            }

            Input = Mapper.Map<UserClaimInputModel>(apiResourceClaim);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string userId, [FromRoute] string id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await unitOfWork.GetObjectByKeyAsync<XpoIdentityUser>(userId);
                    if (user == null)
                    {
                        StatusMessage = "Error: cannot find user";
                        return Page();
                    }

                    var apiResourceClaim = user.Claims.FirstOrDefault(user => user.Id == id);

                    if (apiResourceClaim == null)
                    {
                        StatusMessage = "Error: cannot find user claim";
                        return Page();
                    }

                    apiResourceClaim = Mapper.Map(Input, apiResourceClaim);

                    await unitOfWork.SaveAsync(user);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/Users/Edit/{userId}?SelectedPage=Claims");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving User with {userId}", userId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving User with {userId}", userId);
                    StatusMessage = $"Error saving user: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
