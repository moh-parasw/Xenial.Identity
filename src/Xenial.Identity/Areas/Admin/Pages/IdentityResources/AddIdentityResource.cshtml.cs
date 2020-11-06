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

using Xenial.Identity.Configuration;
using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.IdentityResources
{
    public class AddIdentityResourceModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddIdentityResourceModel> logger;
        public AddIdentityResourceModel(UnitOfWork unitOfWork, ILogger<AddIdentityResourceModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class IdentityResourceInputModel
        {
            [Required]
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; } = true;
            public bool Required { get; set; }
            public bool Emphasize { get; set; }
            public bool ShowInDiscoveryDocument { get; set; } = true;
            public bool NonEditable { get; set; }
            public string UserClaims { get; set; }
        }

        internal class IdentityResourceMappingConfiguration : Profile
        {
            public IdentityResourceMappingConfiguration()
                => CreateMap<IdentityResourceInputModel, XpoIdentityResource>()
                    .ForMember(api => api.UserClaims, o => o.Ignore())
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public IdentityResourceInputModel Input { get; set; } = new IdentityResourceInputModel();

        public string StatusMessage { get; set; }
        public string UserClaims { get; set; }

        public async Task<IActionResult> OnGet()
        {
            await FetchUserClaims();
            return Page();
        }

        private async Task FetchUserClaims()
        {
            var userClaims = await unitOfWork.Query<Xenial.AspNetIdentity.Xpo.Models.XpoIdentityUserClaim>().Select(claim => claim.Type).Distinct().ToListAsync();
            UserClaims = string.Join(",", ClientConstants.StandardClaims.Concat(userClaims).Distinct());
        }

        public async Task<IActionResult> OnPost()
        {
            await FetchUserClaims();
            if (ModelState.IsValid)
            {
                try
                {
                    var identityResource = Mapper.Map(Input, new XpoIdentityResource(unitOfWork));

                    var userClaimsString = string.IsNullOrEmpty(Input.UserClaims) ? string.Empty : Input.UserClaims;
                    var userClaims = userClaimsString.Split(",").Select(s => s.Trim()).ToList();
                    identityResource.UserClaims.AddRange(userClaims.Select(userClaim => new XpoIdentityResourceClaim(unitOfWork)
                    {
                        Type = userClaim
                    }));

                    await unitOfWork.SaveAsync(identityResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/IdentityResources");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving IdentityResource with {Name}", Input?.Name);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Name)}", "Identity resource name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving IdentityResource with {Name}", Input?.Name);
                    StatusMessage = $"Error saving api resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
