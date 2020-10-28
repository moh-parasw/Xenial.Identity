using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
    public class DeleteApiResourceModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteApiResourceModel> logger;
        public DeleteApiResourceModel(UnitOfWork unitOfWork, ILogger<DeleteApiResourceModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiResourceOutputModel
        {
            public string Name { get; set; }
        }

        public ApiResourceOutputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(id);
            if (apiResource == null)
            {
                StatusMessage = "Cannot find api resource";
                return Page();
            }
            if (apiResource != null)
            {
                Input = new ApiResourceOutputModel
                {
                    Name = apiResource.Name
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            if (ModelState.IsValid)
            {
                var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(id);
                if (apiResource == null)
                {
                    StatusMessage = "Error: Cannot find api resource";
                    return Page();
                }

                try
                {
                    await unitOfWork.DeleteAsync(apiResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/ApiResources");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error deleting ApiResource with {Name}", Input?.Name);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Name)}", "Api resource name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error deleting ApiResource with {Name}", Input?.Name);
                    StatusMessage = $"Error saving api resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
