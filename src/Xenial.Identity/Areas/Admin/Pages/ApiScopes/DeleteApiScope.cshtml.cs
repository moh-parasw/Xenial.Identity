using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.ApiScopes
{
    public class DeleteApiScopeModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteApiScopeModel> logger;
        public DeleteApiScopeModel(UnitOfWork unitOfWork, ILogger<DeleteApiScopeModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiScopeOutputModel
        {
            public string Name { get; set; }
        }

        public ApiScopeOutputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            var apiScope = await unitOfWork.GetObjectByKeyAsync<XpoApiScope>(id);
            if (apiScope == null)
            {
                StatusMessage = "Cannot find api Scope";
                return Page();
            }
            if (apiScope != null)
            {
                Input = new ApiScopeOutputModel
                {
                    Name = apiScope.Name
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id)
        {
            if (ModelState.IsValid)
            {
                var apiScope = await unitOfWork.GetObjectByKeyAsync<XpoApiScope>(id);
                if (apiScope == null)
                {
                    StatusMessage = "Error: Cannot find api Scope";
                    return Page();
                }

                try
                {
                    await unitOfWork.DeleteAsync(apiScope);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/ApiScopes");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error deleting ApiScope with {Name}", Input?.Name);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Name)}", "Api Scope name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error deleting ApiScope with {Name}", Input?.Name);
                    StatusMessage = $"Error saving api Scope: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
