using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Users.Claims
{
    public class DeleteUserClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteUserClaimModel> logger;
        public DeleteUserClaimModel(UnitOfWork unitOfWork, ILogger<DeleteUserClaimModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class UserClaimOutputModel
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        internal class UserMappingConfiguration : Profile
        {
            public UserMappingConfiguration()
                => CreateMap<UserClaimOutputModel, XpoIdentityUserClaim>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<UserMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public UserClaimOutputModel Output { get; set; } = new UserClaimOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string userId, [FromRoute] string id)
        {
            var user = await unitOfWork.GetObjectByKeyAsync<XpoIdentityUser>(userId);
            if (user == null)
            {
                StatusMessage = "Error: cannot find user";
                return Page();
            }

            var claim = user.Claims.FirstOrDefault(claim => claim.Id == id);
            if (claim == null)
            {
                StatusMessage = "Error: cannot find user claim";
                return Page();
            }

            Output = Mapper.Map<UserClaimOutputModel>(claim);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string userId, [FromRoute] string id)
        {
            try
            {
                var user = await unitOfWork.GetObjectByKeyAsync<XpoIdentityUser>(userId);
                if (user == null)
                {
                    StatusMessage = "Error: cannot find user";
                    return Page();
                }

                var claim = user.Claims.FirstOrDefault(claim => claim.Id == id);

                if (claim == null)
                {
                    StatusMessage = "Error: cannot find user claim";
                    return Page();
                }

                await unitOfWork.DeleteAsync(claim);
                await unitOfWork.SaveAsync(user);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/Users/Edit/{userId}?SelectedPage=Claims");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving UserClaim with {userId} and {id}", userId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting UserClaim with {userId} and {id}", userId, id);
                StatusMessage = $"Error deleting user claim: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
