using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients.Properties
{
    public class EditClientPropertyModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<EditClientPropertyModel> logger;
        public EditClientPropertyModel(UnitOfWork unitOfWork, ILogger<EditClientPropertyModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientPropertyInputModel
        {
            [Required]
            public string Key { get; set; }
            [Required]
            public string Value { get; set; }
        }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientPropertyInputModel, XpoClientProperty>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientPropertyInputModel Input { get; set; } = new ClientPropertyInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int clientId, [FromRoute] int id)
        {
            var client = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
            if (client == null)
            {
                StatusMessage = "Error: cannot find client";
                return Page();
            }

            var clientProperty = client.Properties.FirstOrDefault(property => property.Id == id);
            if (clientProperty == null)
            {
                StatusMessage = "Error: cannot find client property";
                return Page();
            }

            Input = Mapper.Map<ClientPropertyInputModel>(clientProperty);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int clientId, [FromRoute] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = await unitOfWork.GetObjectByKeyAsync<XpoClient>(clientId);
                    if (client == null)
                    {
                        StatusMessage = "Error: cannot find client";
                        return Page();
                    }

                    var clientProperty = client.Properties.FirstOrDefault(property => property.Id == id);

                    if (clientProperty == null)
                    {
                        StatusMessage = "Error: cannot find client property";
                        return Page();
                    }

                    _ = Mapper.Map(Input, clientProperty);

                    await unitOfWork.SaveAsync(client);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/Clients/Edit/{clientId}?SelectedPage=Properties");
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
