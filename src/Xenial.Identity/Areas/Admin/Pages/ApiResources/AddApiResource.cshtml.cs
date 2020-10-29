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

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources
{
    public class AddApiResourceModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddApiResourceModel> logger;
        public AddApiResourceModel(UnitOfWork unitOfWork, ILogger<AddApiResourceModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiResourceInputModel
        {
            [Required]
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; } = true;
            public bool ShowInDiscoveryDocument { get; set; } = true;
            public bool NonEditable { get; set; }
        }

        internal class ApiResourceMappingConfiguration : Profile
        {
            public ApiResourceMappingConfiguration()
                => CreateMap<ApiResourceInputModel, XpoApiResource>();
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiResourceInputModel Input { get; set; } = new ApiResourceInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var apiResource = Mapper.Map(Input, new XpoApiResource(unitOfWork));
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
