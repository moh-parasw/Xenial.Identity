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

namespace Xenial.Identity.Areas.Admin.Pages.Roles.Claims
{
    public class DeleteRoleClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteRoleClaimModel> logger;
        public DeleteRoleClaimModel(UnitOfWork unitOfWork, ILogger<DeleteRoleClaimModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class RoleClaimOutputModel
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        internal class RoleMappingConfiguration : Profile
        {
            public RoleMappingConfiguration()
                => CreateMap<RoleClaimOutputModel, XpoIdentityRoleClaim>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<RoleMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public RoleClaimOutputModel Output { get; set; } = new RoleClaimOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string roleId, [FromRoute] string id)
        {
            var role = await unitOfWork.GetObjectByKeyAsync<XpoIdentityRole>(roleId);
            if (role == null)
            {
                StatusMessage = "Error: cannot find role";
                return Page();
            }

            var claim = role.Claims.FirstOrDefault(claim => claim.Id == id);
            if (claim == null)
            {
                StatusMessage = "Error: cannot find role claim";
                return Page();
            }

            Output = Mapper.Map<RoleClaimOutputModel>(claim);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string roleId, [FromRoute] string id)
        {
            try
            {
                var role = await unitOfWork.GetObjectByKeyAsync<XpoIdentityRole>(roleId);
                if (role == null)
                {
                    StatusMessage = "Error: cannot find role";
                    return Page();
                }

                var claim = role.Claims.FirstOrDefault(claim => claim.Id == id);

                if (claim == null)
                {
                    StatusMessage = "Error: cannot find role claim";
                    return Page();
                }

                await unitOfWork.DeleteAsync(claim);
                await unitOfWork.SaveAsync(role);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/Roles/Edit/{roleId}?SelectedPage=Claims");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving RoleClaim with {roleId} and {id}", roleId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting RoleClaim with {roleId} and {id}", roleId, id);
                StatusMessage = $"Error deleting role claim: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
