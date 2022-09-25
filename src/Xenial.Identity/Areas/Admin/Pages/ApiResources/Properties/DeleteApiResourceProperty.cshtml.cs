using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources.Properties
{
    public class DeleteApiResourcePropertyModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteApiResourcePropertyModel> logger;
        public DeleteApiResourcePropertyModel(UnitOfWork unitOfWork, ILogger<DeleteApiResourcePropertyModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class ApiResourcePropertyOutputModel
        {
            [Required]
            public string Key { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class ApiResourceMappingConfiguration : Profile
        {
            public ApiResourceMappingConfiguration()
                => CreateMap<ApiResourcePropertyOutputModel, XpoApiResourceProperty>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiResourcePropertyOutputModel Output { get; set; } = new ApiResourcePropertyOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int resourceId, [FromRoute] int id)
        {
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(resourceId);
            if (apiResource == null)
            {
                StatusMessage = "Error: cannot find api resource";
                return Page();
            }

            var apiResourceProperty = apiResource.Properties.FirstOrDefault(property => property.Id == id);
            if (apiResourceProperty == null)
            {
                StatusMessage = "Error: cannot find api resource property";
                return Page();
            }

            Output = Mapper.Map<ApiResourcePropertyOutputModel>(apiResourceProperty);

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

                var apiResourceProperty = apiResource.Properties.FirstOrDefault(property => property.Id == id);

                if (apiResourceProperty == null)
                {
                    StatusMessage = "Error: cannot find api resource property";
                    return Page();
                }

                await unitOfWork.DeleteAsync(apiResourceProperty);
                await unitOfWork.SaveAsync(apiResource);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/ApiResources/Edit/{resourceId}?SelectedPage=Properties");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving ApiResourceProperty with {resourceId} and {id}", resourceId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting ApiResourceProperty with {resourceId} and {id}", resourceId, id);
                StatusMessage = $"Error deleting api resource property: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
