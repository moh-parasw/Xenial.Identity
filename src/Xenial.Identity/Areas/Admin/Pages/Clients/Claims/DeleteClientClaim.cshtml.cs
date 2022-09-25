using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients.Claims
{
    public class DeleteClientClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteClientClaimModel> logger;
        public DeleteClientClaimModel(UnitOfWork unitOfWork, ILogger<DeleteClientClaimModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class ClientClaimOutputModel
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientClaimOutputModel, XpoClientClaim>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientClaimOutputModel Output { get; set; } = new ClientClaimOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int clientId, [FromRoute] int id)
        {
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
            if (apiResource == null)
            {
                StatusMessage = "Error: cannot find client";
                return Page();
            }

            var apiResourceClaim = apiResource.Claims.FirstOrDefault(secret => secret.Id == id);
            if (apiResourceClaim == null)
            {
                StatusMessage = "Error: cannot find client claim";
                return Page();
            }

            Output = Mapper.Map<ClientClaimOutputModel>(apiResourceClaim);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int clientId, [FromRoute] int id)
        {
            try
            {
                var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
                if (apiResource == null)
                {
                    StatusMessage = "Error: cannot find client";
                    return Page();
                }

                var apiResourceClaim = apiResource.Claims.FirstOrDefault(secret => secret.Id == id);

                if (apiResourceClaim == null)
                {
                    StatusMessage = "Error: cannot find client claim";
                    return Page();
                }

                await unitOfWork.DeleteAsync(apiResourceClaim);
                await unitOfWork.SaveAsync(apiResource);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/Clients/Edit/{clientId}?SelectedPage=Claims");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving ClientClaim with {clientId} and {id}", clientId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting ClientClaim with {clientId} and {id}", clientId, id);
                StatusMessage = $"Error deleting client claim: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
