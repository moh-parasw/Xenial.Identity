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
            public bool ShowInDiscoveryDocument { get; set; } = true;
            public bool NonEditable { get; set; }
            public string UserClaims { get; set; }
            public string IdentityScopes { get; set; }
        }

        public IList<SecretsOutputModel> Secrets { get; set; } = new List<SecretsOutputModel>();
        public IList<PropertiesOutputModel> Properties { get; set; } = new List<PropertiesOutputModel>();

        public class SecretsOutputModel
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
            public DateTime Created { get; set; }
            public DateTime? Expiration { get; set; }
        }

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
                                   .ForMember(api => api.Scopes, o => o.Ignore())
                                   .ForMember(api => api.Secrets, o => o.Ignore())
                                   .ForMember(api => api.Properties, o => o.Ignore())
                                   .ReverseMap()
                               ;

                CreateMap<SecretsOutputModel, XpoIdentityResourceSecret>()
                    .ReverseMap();

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

        public string IdentityScopes { get; set; }
        public int Id { get; set; }
        public string SelectedPage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id, [FromQuery] string selectedPage)
        {
            Id = id;
            SelectedPage = selectedPage;

            await FetchScopes();
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(id);
            if (apiResource == null)
            {
                StatusMessage = "Error: Cannot find api resource";
                return Page();
            }

            Input = Mapper.Map<IdentityResourceInputModel>(apiResource);

            Input.UserClaims = string.Join(",", apiResource.UserClaims.Select(userClaim => userClaim.Type));
            Input.IdentityScopes = string.Join(",", apiResource.Scopes.Select(scope => scope.Scope));

            Secrets = apiResource.Secrets.Select(secret => Mapper.Map<SecretsOutputModel>(secret)).ToList();
            Properties = apiResource.Properties.Select(propertey => Mapper.Map<PropertiesOutputModel>(propertey)).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            Id = id;
            await FetchScopes();
            if (ModelState.IsValid)
            {
                try
                {
                    var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(id);
                    if (apiResource == null)
                    {
                        StatusMessage = "Error: Cannot find api resource";
                        return Page();
                    }

                    foreach (var userClaim in apiResource.UserClaims.ToList())
                    {
                        apiResource.UserClaims.Remove(userClaim);
                    }

                    var userClaimsString = string.IsNullOrEmpty(Input.UserClaims) ? string.Empty : Input.UserClaims;
                    var userClaims = userClaimsString.Split(",").Select(s => s.Trim()).ToList();
                    apiResource.UserClaims.AddRange(userClaims.Select(userClaim => new XpoIdentityResourceClaim(unitOfWork)
                    {
                        Type = userClaim
                    }));

                    foreach (var scope in apiResource.Scopes.ToList())
                    {
                        apiResource.Scopes.Remove(scope);
                    }

                    var apiScopesString = string.IsNullOrEmpty(Input.IdentityScopes) ? string.Empty : Input.IdentityScopes;
                    var apiScopes = apiScopesString.Split(",").Select(s => s.Trim()).ToList();
                    apiResource.Scopes.AddRange(apiScopes.Select(scope => new XpoIdentityResourceScope(unitOfWork)
                    {
                        Scope = scope
                    }));

                    await unitOfWork.SaveAsync(apiResource);
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
