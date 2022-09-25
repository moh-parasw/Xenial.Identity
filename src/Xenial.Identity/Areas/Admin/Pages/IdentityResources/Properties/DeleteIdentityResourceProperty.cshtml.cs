using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.IdentityResources.Properties
{
    public class DeleteIdentityResourcePropertyModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteIdentityResourcePropertyModel> logger;
        public DeleteIdentityResourcePropertyModel(UnitOfWork unitOfWork, ILogger<DeleteIdentityResourcePropertyModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class IdentityResourcePropertyOutputModel
        {
            [Required]
            public string Key { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class IdentityResourceMappingConfiguration : Profile
        {
            public IdentityResourceMappingConfiguration()
                => CreateMap<IdentityResourcePropertyOutputModel, XpoIdentityResourceProperty>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public IdentityResourcePropertyOutputModel Output { get; set; } = new IdentityResourcePropertyOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int resourceId, [FromRoute] int id)
        {
            var IdentityResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(resourceId);
            if (IdentityResource == null)
            {
                StatusMessage = "Error: cannot find Identity resource";
                return Page();
            }

            var IdentityResourceProperty = IdentityResource.Properties.FirstOrDefault(property => property.Id == id);
            if (IdentityResourceProperty == null)
            {
                StatusMessage = "Error: cannot find Identity resource property";
                return Page();
            }

            Output = Mapper.Map<IdentityResourcePropertyOutputModel>(IdentityResourceProperty);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int resourceId, [FromRoute] int id)
        {
            try
            {
                var IdentityResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(resourceId);
                if (IdentityResource == null)
                {
                    StatusMessage = "Error: cannot find Identity resource";
                    return Page();
                }

                var IdentityResourceProperty = IdentityResource.Properties.FirstOrDefault(property => property.Id == id);

                if (IdentityResourceProperty == null)
                {
                    StatusMessage = "Error: cannot find Identity resource property";
                    return Page();
                }

                await unitOfWork.DeleteAsync(IdentityResourceProperty);
                await unitOfWork.SaveAsync(IdentityResource);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/IdentityResources/Edit/{resourceId}?SelectedPage=Properties");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving IdentityResourceProperty with {resourceId} and {id}", resourceId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting IdentityResourceProperty with {resourceId} and {id}", resourceId, id);
                StatusMessage = $"Error deleting Identity resource property: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
