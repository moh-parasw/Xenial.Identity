using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Configuration;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.IdentityResources
{
    public class EditIdentityResourceModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditIdentityResourceModel> logger;
        public EditIdentityResourceModel(UnitOfWork unitOfWork, ILogger<EditIdentityResourceModel> logger)
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

        public IList<PropertiesOutputModel> Properties { get; set; } = new List<PropertiesOutputModel>();

        public class PropertiesOutputModel
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        internal class IdentityResourceMappingConfiguration : Profile
        {
            public IdentityResourceMappingConfiguration()
            {
                CreateMap<IdentityResourceInputModel, XpoIdentityResource>()
                                   .ForMember(api => api.UserClaims, o => o.Ignore())
                                   .ForMember(api => api.Properties, o => o.Ignore())
                                   .ReverseMap()
                               ;

                CreateMap<PropertiesOutputModel, XpoIdentityResourceProperty>()
                    .ReverseMap();
            }
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public IdentityResourceInputModel Input { get; set; } = new IdentityResourceInputModel();

        public string StatusMessage { get; set; }
        public int Id { get; set; }
        public string SelectedPage { get; set; }
        public string UserClaims { get; set; }

        private async Task FetchUserClaims()
        {
            var userClaims = await unitOfWork.Query<Xenial.AspNetIdentity.Xpo.Models.XpoIdentityUserClaim>().Select(claim => claim.Type).Distinct().ToListAsync();
            UserClaims = string.Join(",", ClientConstants.StandardClaims.Concat(userClaims).Distinct());
        }

        public async Task<IActionResult> OnGet([FromRoute] int id, [FromQuery] string selectedPage)
        {
            Id = id;
            SelectedPage = selectedPage;
            await FetchUserClaims();

            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(id);
            if (apiResource == null)
            {
                StatusMessage = "Error: Cannot find api resource";
                return Page();
            }

            Input = Mapper.Map<IdentityResourceInputModel>(apiResource);

            Input.UserClaims = string.Join(",", apiResource.UserClaims.Select(userClaim => userClaim.Type));
            Properties = apiResource.Properties.Select(propertey => Mapper.Map<PropertiesOutputModel>(propertey)).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            Id = id;
            await FetchUserClaims();
            if (ModelState.IsValid)
            {
                try
                {
                    var identityResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(id);
                    if (identityResource == null)
                    {
                        StatusMessage = "Error: Cannot find api resource";
                        return Page();
                    }
                    identityResource = Mapper.Map(Input, identityResource);
                    foreach (var userClaim in identityResource.UserClaims.ToList())
                    {
                        identityResource.UserClaims.Remove(userClaim);
                    }

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
