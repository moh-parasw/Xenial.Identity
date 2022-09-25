using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.IdentityResources.Properties
{
    public class EditIdentityResourcePropertyModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditIdentityResourcePropertyModel> logger;
        public EditIdentityResourcePropertyModel(UnitOfWork unitOfWork, ILogger<EditIdentityResourcePropertyModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class IdentityResourcePropertyInputModel
        {
            [Required]
            public string Key { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class IdentityResourceMappingConfiguration : Profile
        {
            public IdentityResourceMappingConfiguration()
                => CreateMap<IdentityResourcePropertyInputModel, XpoIdentityResourceProperty>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public IdentityResourcePropertyInputModel Input { get; set; } = new IdentityResourcePropertyInputModel();

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

            Input = Mapper.Map<IdentityResourcePropertyInputModel>(IdentityResourceProperty);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int resourceId, [FromRoute] int id)
        {
            if (ModelState.IsValid)
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

                    _ = Mapper.Map(Input, IdentityResourceProperty);

                    await unitOfWork.SaveAsync(IdentityResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/IdentityResources/Edit/{resourceId}?SelectedPage=Properties");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving IdentityResource with {resourceId}", resourceId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving IdentityResource with {resourceId}", resourceId);
                    StatusMessage = $"Error saving Identity resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
