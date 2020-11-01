using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using IdentityServer4.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Configuration;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients.Secrets
{
    public class AddClientSecretModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddClientSecretModel> logger;
        public AddClientSecretModel(UnitOfWork unitOfWork, ILogger<AddClientSecretModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientSecretInputModel
        {
            [Required]
            public string Type { get; set; }
            [Required]
            public string Value { get; set; } = IdentityModel.CryptoRandom.CreateUniqueId();
            public string Description { get; set; }
            public DateTime? Expiration { get; set; }
            public HashTypes HashType { get; set; }
            public enum HashTypes
            {
                Sha256,
                Sha512
            }
        }

        public readonly SelectList SecretTypes = new SelectList(ClientConstants.SecretTypes);
        public readonly SelectList HashTypes = new SelectList(Enum.GetValues(typeof(ClientSecretInputModel.HashTypes)));

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

        private void HashApiSharedSecret(ClientSecretInputModel inputModel)
        {
            if (inputModel.Type != ClientConstants.SharedSecret)
            {
                return;
            }

            if (inputModel.HashType == ClientSecretInputModel.HashTypes.Sha256)
            {
                inputModel.Value = inputModel.Value.Sha256();
            }
            else if (inputModel.HashType == ClientSecretInputModel.HashTypes.Sha512)
            {
                inputModel.Value = inputModel.Value.Sha512();
            }
        }

        public async Task<IActionResult> OnPost([FromRoute] int clientId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
                    if (apiResource == null)
                    {
                        StatusMessage = "Error: cannot find api resource";
                        return Page();
                    }

                    HashApiSharedSecret(Input);

                    apiResource.ClientSecrets.Add(Mapper.Map(Input, new XpoClientSecret(unitOfWork)));

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
                    StatusMessage = $"Error saving api resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
