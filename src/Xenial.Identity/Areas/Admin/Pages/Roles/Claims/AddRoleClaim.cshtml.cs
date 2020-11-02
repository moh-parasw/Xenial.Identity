using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using IdentityServer4.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Configuration;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Roles.Claims
{
    public class AddRoleClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddRoleClaimModel> logger;
        public AddRoleClaimModel(UnitOfWork unitOfWork, ILogger<AddRoleClaimModel> logger)
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

        public async Task<IActionResult> OnPost([FromRoute] string roleId)
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

                    role.Claims.Add(Mapper.Map(Input, new XpoIdentityRoleClaim(unitOfWork)
                    {
                        Id = Guid.NewGuid().ToString()
                    }));

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
