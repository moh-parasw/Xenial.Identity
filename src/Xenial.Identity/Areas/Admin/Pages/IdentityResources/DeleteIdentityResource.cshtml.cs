using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.IdentityResources
{
    public class DeleteIdentityResourceModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteIdentityResourceModel> logger;
        public DeleteIdentityResourceModel(UnitOfWork unitOfWork, ILogger<DeleteIdentityResourceModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class IdentityResourceOutputModel
        {
            public string Name { get; set; }
        }

        public IdentityResourceOutputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            var identityResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(id);
            if (identityResource == null)
            {
                StatusMessage = "Cannot find identity resource";
                return Page();
            }
            if (identityResource != null)
            {
                Input = new IdentityResourceOutputModel
                {
                    Name = identityResource.Name
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            if (ModelState.IsValid)
            {
                var identityResource = await unitOfWork.GetObjectByKeyAsync<XpoIdentityResource>(id);
                if (identityResource == null)
                {
                    StatusMessage = "Error: Cannot find identity resource";
                    return Page();
                }

                try
                {
                    await unitOfWork.DeleteAsync(identityResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/IdentityResources");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error deleting IdentityResource with {Name}", Input?.Name);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Name)}", "Identity resource name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error deleting IdentityResource with {Name}", Input?.Name);
                    StatusMessage = $"Error saving identity resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
