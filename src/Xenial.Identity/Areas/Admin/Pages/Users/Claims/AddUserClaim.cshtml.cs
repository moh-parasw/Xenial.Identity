using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Users.Claims
{
    public class AddUserClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddUserClaimModel> logger;
        public AddUserClaimModel(UnitOfWork unitOfWork, ILogger<AddUserClaimModel> logger)
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

        public async Task<IActionResult> OnPost([FromRoute] string userId)
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

                    user.Claims.Add(Mapper.Map(Input, new XpoIdentityUserClaim(unitOfWork)
                    {
                        Id = Guid.NewGuid().ToString()
                    }));

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
