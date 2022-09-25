using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients.Claims
{
    public class AddClientClaimModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddClientClaimModel> logger;
        public AddClientClaimModel(UnitOfWork unitOfWork, ILogger<AddClientClaimModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientClaimInputModel
        {
            [Required]
            public string Type { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientClaimInputModel, XpoClientClaim>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientClaimInputModel Input { get; set; } = new ClientClaimInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPost([FromRoute] int clientId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
                    if (apiResource == null)
                    {
                        StatusMessage = "Error: cannot find client";
                        return Page();
                    }

                    apiResource.Claims.Add(Mapper.Map(Input, new XpoClientClaim(unitOfWork)));

                    await unitOfWork.SaveAsync(apiResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/Clients/Edit/{clientId}?SelectedPage=Claims");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving Client with {clientId}", clientId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving Client with {clientId}", clientId);
                    StatusMessage = $"Error saving client: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
