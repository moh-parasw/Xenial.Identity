using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.ApiScopes
{
    public class ViewApiScopeModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<ViewApiScopeModel> logger;
        public ViewApiScopeModel(UnitOfWork unitOfWork, ILogger<ViewApiScopeModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiScopeOutputModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; } = true;
            public bool ShowInDiscoveryDocument { get; set; } = true;
            public bool NonEditable { get; set; }
            public string UserClaims { get; set; }
        }

        internal class ApiScopeMappingConfiguration : Profile
        {
            public ApiScopeMappingConfiguration()
                => CreateMap<ApiScopeOutputModel, XpoApiScope>()
                    .ForMember(api => api.UserClaims, o => o.Ignore())
                    .ReverseMap();
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiScopeMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiScopeOutputModel Output { get; set; } = new ApiScopeOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            var scope = await unitOfWork.GetObjectByKeyAsync<XpoApiScope>(id);
            if (scope == null)
            {
                StatusMessage = "Error: Cannot find api scope";
                return Page();
            }

            Output = Mapper.Map<ApiScopeOutputModel>(scope);
            Output.UserClaims = string.Join(",", scope.UserClaims.Select(userClaim => userClaim.Type));

            return Page();
        }
    }
}
