using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Roles.Claims
{
    public class EditRoleClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditRoleClaimModel> logger;
        public EditRoleClaimModel(UnitOfWork unitOfWork, ILogger<EditRoleClaimModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class RoleClaimInputModel
        {
            [Required]
            public string Type { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class RoleMappingConfiguration : Profile
        {
            public RoleMappingConfiguration()
                => CreateMap<RoleClaimInputModel, XpoIdentityRoleClaim>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<RoleMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public RoleClaimInputModel Input { get; set; } = new RoleClaimInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string roleId, [FromRoute] string id)
        {
            var role = await unitOfWork.GetObjectByKeyAsync<XpoIdentityRole>(roleId);
            if (role == null)
            {
                StatusMessage = "Error: cannot find role";
                return Page();
            }

            var apiResourceClaim = role.Claims.FirstOrDefault(role => role.Id == id);
            if (apiResourceClaim == null)
            {
                StatusMessage = "Error: cannot find role claim";
                return Page();
            }

            Input = Mapper.Map<RoleClaimInputModel>(apiResourceClaim);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string roleId, [FromRoute] string id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var role = await unitOfWork.GetObjectByKeyAsync<XpoIdentityRole>(roleId);
                    if (role == null)
                    {
                        StatusMessage = "Error: cannot find role";
                        return Page();
                    }

                    var apiResourceClaim = role.Claims.FirstOrDefault(role => role.Id == id);

                    if (apiResourceClaim == null)
                    {
                        StatusMessage = "Error: cannot find role claim";
                        return Page();
                    }

                    apiResourceClaim = Mapper.Map(Input, apiResourceClaim);

                    await unitOfWork.SaveAsync(role);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/Roles/Edit/{roleId}?SelectedPage=Claims");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving Role with {roleId}", roleId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving Role with {roleId}", roleId);
                    StatusMessage = $"Error saving role: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
