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

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.IdentityResources.Properties
{
    public class AddIdentityResourcePropertyModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddIdentityResourcePropertyModel> logger;
        public AddIdentityResourcePropertyModel(UnitOfWork unitOfWork, ILogger<AddIdentityResourcePropertyModel> logger)
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

        public async Task<IActionResult> OnPost([FromRoute] int resourceId)
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

                    IdentityResource.Properties.Add(Mapper.Map(Input, new XpoIdentityResourceProperty(unitOfWork)));

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
