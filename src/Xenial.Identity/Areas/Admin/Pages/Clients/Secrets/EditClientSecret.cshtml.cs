using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients.Secrets
{
    public class EditClientSecretModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditClientSecretModel> logger;
        public EditClientSecretModel(UnitOfWork unitOfWork, ILogger<EditClientSecretModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientSecretInputModel
        {
            public string Description { get; set; }
            public DateTime? Expiration { get; set; }
        }

        public string Type { get; set; }
        public string Value { get; set; }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientSecretInputModel, XpoClientSecret>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientSecretInputModel Input { get; set; } = new ClientSecretInputModel();

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

            Input = Mapper.Map<ClientSecretInputModel>(apiResourceSecret);
            Type = apiResourceSecret.Type;
            Value = apiResourceSecret.Value;

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int clientId, [FromRoute] int id)
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

                    var apiResourceSecret = apiResource.ClientSecrets.FirstOrDefault(secret => secret.Id == id);

                    if (apiResourceSecret == null)
                    {
                        StatusMessage = "Error: cannot find client secret";
                        return Page();
                    }

                    apiResourceSecret.Expiration = Input.Expiration;
                    apiResourceSecret.Description = Input.Description;

                    await unitOfWork.SaveAsync(apiResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/Clients/Edit/{clientId}?SelectedPage=Secrets");
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
