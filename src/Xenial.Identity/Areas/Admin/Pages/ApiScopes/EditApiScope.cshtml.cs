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

namespace Xenial.Identity.Areas.Admin.Pages.ApiScopes
{
    public class EditApiScopeModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditApiScopeModel> logger;
        public EditApiScopeModel(UnitOfWork unitOfWork, ILogger<EditApiScopeModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiScopeInputModel
        {
            [Required]
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; } = true;
            public bool Required { get; set; } = true;
            public bool Emphasize { get; set; }
            public bool ShowInDiscoveryDocument { get; set; } = true;
            public bool NonEditable { get; set; }
            public string UserClaims { get; set; } = "profile";
        }

        internal class ApiScopeMappingConfiguration : Profile
        {
            public ApiScopeMappingConfiguration()
                => CreateMap<ApiScopeInputModel, XpoApiScope>()
                    .ForMember(api => api.UserClaims, o => o.Ignore())
                    .ReverseMap();
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiScopeMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiScopeInputModel Input { get; set; } = new ApiScopeInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            var scope = await unitOfWork.GetObjectByKeyAsync<XpoApiScope>(id);
            if (scope == null)
            {
                StatusMessage = "Error: Cannot find api scope";
                return Page();
            }

            Input = Mapper.Map<ApiScopeInputModel>(scope);
            Input.UserClaims = string.Join(",", scope.UserClaims.Select(userClaim => userClaim.Type));

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var scope = await unitOfWork.GetObjectByKeyAsync<XpoApiScope>(id);
                    if (scope == null)
                    {
                        StatusMessage = "Error: Cannot find api scope";
                        return Page();
                    }

                    var apiScope = Mapper.Map(Input, scope);

                    var userClaimString = string.IsNullOrEmpty(Input.UserClaims) ? string.Empty : Input.UserClaims;
                    var userClaims = userClaimString.Split(",").Select(s => s.Trim()).ToList();

                    foreach (var userClaim in apiScope.UserClaims.ToList())
                    {
                        apiScope.UserClaims.Remove(userClaim);
                    }

                    apiScope.UserClaims.AddRange(userClaims.Select(userClaim => new XpoApiScopeClaim(unitOfWork)
                    {
                        Type = userClaim
                    }));

                    await unitOfWork.SaveAsync(apiScope);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/ApiScopes");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving ApiScope with {Name}", Input?.Name);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Name)}", "Api Scope name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving ApiScope with {Name}", Input?.Name);
                    StatusMessage = $"Error saving api Scope: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
