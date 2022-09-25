using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients.Secrets
{
    public class DeleteClientSecretModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteClientSecretModel> logger;
        public DeleteClientSecretModel(UnitOfWork unitOfWork, ILogger<DeleteClientSecretModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class ClientSecretOutputModel
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public string Description { get; set; }
        }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientSecretOutputModel, XpoClientSecret>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientSecretOutputModel Output { get; set; } = new ClientSecretOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int clientId, [FromRoute] int id)
        {
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
            if (apiResource == null)
            {
                StatusMessage = "Error: cannot find client";
                return Page();
            }

            var apiResourceSecret = apiResource.ClientSecrets.FirstOrDefault(secret => secret.Id == id);
            if (apiResourceSecret == null)
            {
                StatusMessage = "Error: cannot find client secret";
                return Page();
            }

            Output = Mapper.Map<ClientSecretOutputModel>(apiResourceSecret);

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

                var apiResourceSecret = apiResource.ClientSecrets.FirstOrDefault(secret => secret.Id == id);

                if (apiResourceSecret == null)
                {
                    StatusMessage = "Error: cannot find client secret";
                    return Page();
                }

                await unitOfWork.DeleteAsync(apiResourceSecret);
                await unitOfWork.SaveAsync(apiResource);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/Clients/Edit/{clientId}?SelectedPage=Secrets");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving ClientSecret with {clientId} and {id}", clientId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting ClientSecret with {clientId} and {id}", clientId, id);
                StatusMessage = $"Error deleting client secret: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
