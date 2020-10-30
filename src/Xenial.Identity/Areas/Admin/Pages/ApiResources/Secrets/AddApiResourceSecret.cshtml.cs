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

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources.Secrets
{
    public class AddApiResourceSecretModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddApiResourceSecretModel> logger;
        public AddApiResourceSecretModel(UnitOfWork unitOfWork, ILogger<AddApiResourceSecretModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ApiResourceSecretInputModel
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
        public readonly SelectList HashTypes = new SelectList(Enum.GetValues(typeof(ApiResourceSecretInputModel.HashTypes)));

        internal class ApiResourceMappingConfiguration : Profile
        {
            public ApiResourceMappingConfiguration()
                => CreateMap<ApiResourceSecretInputModel, XpoApiResourceSecret>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ApiResourceSecretInputModel Input { get; set; } = new ApiResourceSecretInputModel();

        public string StatusMessage { get; set; }

        private void HashApiSharedSecret(ApiResourceSecretInputModel inputModel)
        {
            if (inputModel.Type != ClientConstants.SharedSecret)
            {
                return;
            }

            if (inputModel.HashType == ApiResourceSecretInputModel.HashTypes.Sha256)
            {
                inputModel.Value = inputModel.Value.Sha256();
            }
            else if (inputModel.HashType == ApiResourceSecretInputModel.HashTypes.Sha512)
            {
                inputModel.Value = inputModel.Value.Sha512();
            }
        }

        public async Task<IActionResult> OnPost([FromRoute] int resourceId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoApiResource>(resourceId);
                    if (apiResource == null)
                    {
                        StatusMessage = "Error: cannot find api resource";
                        return Page();
                    }

                    HashApiSharedSecret(Input);

                    apiResource.Secrets.Add(Mapper.Map(Input, new XpoApiResourceSecret(unitOfWork)));

                    await unitOfWork.SaveAsync(apiResource);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/ApiResources/Edit/{resourceId}?SelectedPage=Secrets");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving ApiResource with {resourceId}", resourceId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving ApiResource with {resourceId}", resourceId);
                    StatusMessage = $"Error saving api resource: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
