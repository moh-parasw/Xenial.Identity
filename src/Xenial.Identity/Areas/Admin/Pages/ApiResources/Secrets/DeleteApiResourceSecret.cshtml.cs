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

using Xenial.Identity.Data;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources.Secrets
{
    public class DeleteApiResourceSecretModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteApiResourceSecretModel> logger;
        public DeleteApiResourceSecretModel(UnitOfWork unitOfWork, ILogger<DeleteApiResourceSecretModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class ApiResourceSecretOutputModel
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public string Description { get; set; }
        }

        internal class ApiResourceMappingConfiguration : Profile
        {
            public ApiResourceMappingConfiguration()
                => CreateMap<ApiResourceSecretOutputModel, XpoApiResourceSecret>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiResourceSecretOutputModel Output { get; set; } = new ApiResourceSecretOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int resourceId, [FromRoute] int id)
        {
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(resourceId);
            if (apiResource == null)
            {
                StatusMessage = "Error: cannot find api resource";
                return Page();
            }

            var apiResourceSecret = apiResource.Secrets.FirstOrDefault(secret => secret.Id == id);
            if (apiResourceSecret == null)
            {
                StatusMessage = "Error: cannot find api resource secret";
                return Page();
            }

            Output = Mapper.Map<ApiResourceSecretOutputModel>(apiResourceSecret);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int resourceId, [FromRoute] int id)
        {
            try
            {
                var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(resourceId);
                if (apiResource == null)
                {
                    StatusMessage = "Error: cannot find api resource";
                    return Page();
                }

                var apiResourceSecret = apiResource.Secrets.FirstOrDefault(secret => secret.Id == id);

                if (apiResourceSecret == null)
                {
                    StatusMessage = "Error: cannot find api resource secret";
                    return Page();
                }

                await unitOfWork.DeleteAsync(apiResourceSecret);
                await unitOfWork.SaveAsync(apiResource);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/ApiResources/Edit/{resourceId}?SelectedPage=Secrets");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving ApiResourceSecret with {resourceId} and {id}", resourceId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting ApiResourceSecret with {resourceId} and {id}", resourceId, id);
                StatusMessage = $"Error deleting api resource secret: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
