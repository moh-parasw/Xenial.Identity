using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients.Properties
{
    public class DeleteClientPropertyModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<DeleteClientPropertyModel> logger;
        public DeleteClientPropertyModel(UnitOfWork unitOfWork, ILogger<DeleteClientPropertyModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);


        public class ClientPropertyOutputModel
        {
            [Required]
            public string Key { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientPropertyOutputModel, XpoClientProperty>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientPropertyOutputModel Output { get; set; } = new ClientPropertyOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int clientId, [FromRoute] int id)
        {
            var apiResource = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
            if (apiResource == null)
            {
                StatusMessage = "Error: cannot find client";
                return Page();
            }

            var apiResourceProperty = apiResource.Properties.FirstOrDefault(property => property.Id == id);
            if (apiResourceProperty == null)
            {
                StatusMessage = "Error: cannot find client property";
                return Page();
            }

            Output = Mapper.Map<ClientPropertyOutputModel>(apiResourceProperty);

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

                var apiResourceProperty = apiResource.Properties.FirstOrDefault(property => property.Id == id);

                if (apiResourceProperty == null)
                {
                    StatusMessage = "Error: cannot find client property";
                    return Page();
                }

                await unitOfWork.DeleteAsync(apiResourceProperty);
                await unitOfWork.SaveAsync(apiResource);
                await unitOfWork.CommitChangesAsync();
                return Redirect($"/Admin/Clients/Edit/{clientId}?SelectedPage=Properties");
            }
            catch (ConstraintViolationException ex)
            {
                logger.LogWarning(ex, "Error saving ClientProperty with {clientId} and {id}", clientId, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting ClientProperty with {clientId} and {id}", clientId, id);
                StatusMessage = $"Error deleting client property: {ex.Message}";
                return Page();
            }


            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}
