using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Duende.IdentityServer.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients
{

    public class AddClientModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddClientModel> logger;
        public AddClientModel(UnitOfWork unitOfWork, ILogger<AddClientModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientInputModel
        {
            [Required]
            public string ClientId { get; set; }
            [Required]
            public string ClientName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            [Required]
            public ClientTypes? ClientType { get; set; }
        }

        public IEnumerable<(ClientTypes, string header, string description, string icon)> GetClientTypes()
        {
            foreach (ClientTypes value in Enum.GetValues(typeof(ClientTypes)))
            {
                var header = value.GetAttributeOfType<HeaderAttribute>().Header;
                var description = value.GetAttributeOfType<DescriptionAttribute>().Description;
                var icon = value.GetAttributeOfType<IconAttribute>().Icon;
                yield return (value, header, description, icon);
            }
        }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientInputModel, XpoClient>()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientInputModel Input { get; set; } = new ClientInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = Mapper.Map(Input, new XpoClient(unitOfWork));

                    PrepareClientTypeForNewClient(client);

                    await unitOfWork.SaveAsync(client);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect($"/Admin/Clients/Edit/{client.Id}?clientType={Input.ClientType}");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving Client with {ClientName}", Input?.ClientName);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.ClientName)}", "Client name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving Client with {ClientName}", Input?.ClientName);
                    StatusMessage = $"Error saving client: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }

        private void PrepareClientTypeForNewClient(XpoClient client)
        {
            switch (Input.ClientType)
            {
                case ClientTypes.Empty:
                    break;
                case ClientTypes.Web:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Hybrid, client));
                    client.RequirePkce = true;
                    client.RequireClientSecret = true;
                    client.AllowOfflineAccess = true;
                    break;
                case ClientTypes.Native:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Hybrid, client));
                    client.RequirePkce = true;
                    client.RequireClientSecret = false;
                    client.AllowOfflineAccess = true;
                    break;
                case ClientTypes.Spa:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Code, client));
                    client.RequirePkce = true;
                    client.RequireClientSecret = false;
                    break;
                case ClientTypes.Machine:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.ClientCredentials, client));
                    break;
                case ClientTypes.Device:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.DeviceFlow, client));
                    client.RequireClientSecret = false;
                    client.AllowOfflineAccess = true;
                    break;
                case ClientTypes.ResourceOwnerPassword:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.ResourceOwnerPassword, client));
                    client.RequireClientSecret = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            static IEnumerable<XpoClientGrantType> CreateGrantTypes(IEnumerable<string> grantTypes, XpoClient client)
            {
                return grantTypes.Select(grant => new XpoClientGrantType(client.Session) { GrantType = grant });
            }
        }
    }
}
