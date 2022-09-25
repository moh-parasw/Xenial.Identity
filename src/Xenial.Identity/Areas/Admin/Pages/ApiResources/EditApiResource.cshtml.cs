using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources
{
    public class EditApiResourceModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditApiResourceModel> logger;
        public EditApiResourceModel(UnitOfWork unitOfWork, ILogger<EditApiResourceModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiResourceInputModel
        {
            [Required]
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; } = true;
            public bool Required { get; set; } = true;
            public bool ShowInDiscoveryDocument { get; set; } = true;
            public bool NonEditable { get; set; }
            public string UserClaims { get; set; }
            public string ApiScopes { get; set; }
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

        internal class ApiResourceMappingConfiguration : Profile
        {
            public ApiResourceMappingConfiguration()
            {
                _ = CreateMap<ApiResourceInputModel, XpoApiResource>()
                                   .ForMember(api => api.UserClaims, o => o.Ignore())
                                   .ForMember(api => api.Scopes, o => o.Ignore())
                                   .ForMember(api => api.Secrets, o => o.Ignore())
                                   .ForMember(api => api.Properties, o => o.Ignore())
                                   .ReverseMap()
                               ;

                _ = CreateMap<SecretsOutputModel, XpoApiResourceSecret>()
                    .ReverseMap();

                _ = CreateMap<PropertiesOutputModel, XpoApiResourceProperty>()
                    .ReverseMap();
            }
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiResourceInputModel Input { get; set; } = new ApiResourceInputModel();

        public string StatusMessage { get; set; }

        public string ApiScopes { get; set; }
        public int Id { get; set; }
        public string SelectedPage { get; set; }

        private async Task FetchScopes()
        {
            var scopeNames = await unitOfWork.Query<XpoApiScope>().Select(scope => scope.Name).ToListAsync();
            ApiScopes = string.Join(",", scopeNames);
        }

        public async Task<IActionResult> OnGet([FromRoute] int id, [FromQuery] string selectedPage)
        {
            Id = id;
            SelectedPage = selectedPage;

            await FetchScopes();
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(id);
            if (apiResource == null)
            {
                StatusMessage = "Error: Cannot find api resource";
                return Page();
            }

            Input = Mapper.Map<ApiResourceInputModel>(apiResource);

            Input.UserClaims = string.Join(",", apiResource.UserClaims.Select(userClaim => userClaim.Type));
            Input.ApiScopes = string.Join(",", apiResource.Scopes.Select(scope => scope.Scope));

            FetchSubLists(apiResource);

            return Page();
        }

        private void FetchSubLists(XpoApiResource apiResource)
        {
            Secrets = apiResource.Secrets.Select(Mapper.Map<SecretsOutputModel>).ToList();
            Properties = apiResource.Properties.Select(Mapper.Map<PropertiesOutputModel>).ToList();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            Id = id;
            await FetchScopes();

            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(id);
            if (apiResource == null)
            {
                StatusMessage = "Error: Cannot find api resource";
                return Page();
            }

            FetchSubLists(apiResource);

            if (ModelState.IsValid)
            {
                try
                {
                    apiResource = Mapper.Map(Input, apiResource);
                    foreach (var userClaim in apiResource.UserClaims.ToList())
                    {
                        _ = apiResource.UserClaims.Remove(userClaim);
                    }

                    var userClaimsString = string.IsNullOrEmpty(Input.UserClaims) ? string.Empty : Input.UserClaims;
                    var userClaims = userClaimsString.Split(",").Select(s => s.Trim()).ToList();
                    apiResource.UserClaims.AddRange(userClaims.Select(userClaim => new XpoApiResourceClaim(unitOfWork)
                    {
                        Type = userClaim
                    }));

                    foreach (var scope in apiResource.Scopes.ToList())
                    {
                        _ = apiResource.Scopes.Remove(scope);
                    }

                    var apiScopesString = string.IsNullOrEmpty(Input.ApiScopes) ? string.Empty : Input.ApiScopes;
                    var apiScopes = apiScopesString.Split(",").Select(s => s.Trim()).ToList();
                    apiResource.Scopes.AddRange(apiScopes.Select(scope => new XpoApiResourceScope(unitOfWork)
                    {
                        Scope = scope
                    }));

                    await unitOfWork.SaveAsync(apiResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/ApiResources");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving ApiResource with {Name}", Input?.Name);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Name)}", "Api resource name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving ApiResource with {Name}", Input?.Name);
                    StatusMessage = $"Error saving api resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
