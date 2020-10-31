using System;
using System.Collections.Generic;
using System.ComponentModel;
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

using IdentityServer4.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients
{
    public class EditClientModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddClientModel> logger;
        public EditClientModel(UnitOfWork unitOfWork, ILogger<AddClientModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientInputModel
        {
            [Required]
            public string ClientId { get; set; }
            [Required]
            public string ClientName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }

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
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientInputModel Input { get; set; } = new ClientInputModel();

        public string StatusMessage { get; set; }

        public ClientTypes? ClientType { get; set; }
        public int Id { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] int id, [FromQuery] ClientTypes? clientType = null)
        {
            Id = id;
            var client = await unitOfWork.GetObjectByKeyAsync<XpoClient>(id);
            if (client == null)
            {
                StatusMessage = "Error: can not find client";
                return Page();
            }

            ClientType = clientType.HasValue ? clientType.Value : GuessClientType(client);
            Input = Mapper.Map(client, Input);

            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] int id, [FromQuery] ClientTypes? clientType = null)
        {
            Id = id;
            if (ModelState.IsValid)
            {
                try
                {
                    var client = await unitOfWork.GetObjectByKeyAsync<XpoClient>(id);
                    if (client == null)
                    {
                        StatusMessage = "Error: can not find client";
                        return Page();
                    }

                    ClientType = clientType.HasValue ? clientType.Value : GuessClientType(client);
                    client = Mapper.Map(Input, client);

                    await unitOfWork.SaveAsync(client);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/Clients");
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

        private static ClientTypes? GuessClientType(XpoClient client)
            => client switch
            {
                XpoClient c
                    when c.RequirePkce
                    && c.RequireClientSecret
                    && c.AllowOfflineAccess
                    && c.AllowedGrantTypes.Count > 0
                    && c.AllowedGrantTypes.Select(g => g.GrantType).All(GrantTypes.Hybrid.Contains)
                        => ClientTypes.Web,

                XpoClient c
                    when c.RequirePkce
                    && !c.RequireClientSecret
                    && c.AllowOfflineAccess
                    && c.AllowedGrantTypes.Count > 0
                    && c.AllowedGrantTypes.Select(g => g.GrantType).All(GrantTypes.Hybrid.Contains)
                        => ClientTypes.Native,

                XpoClient c
                    when c.RequirePkce
                    && !c.RequireClientSecret
                    && c.AllowedGrantTypes.Count > 0
                    && c.AllowedGrantTypes.Select(g => g.GrantType).All(GrantTypes.Code.Contains)
                        => ClientTypes.Spa,

                XpoClient c
                    when c.AllowOfflineAccess
                    && !c.RequireClientSecret
                    && c.AllowedGrantTypes.Count > 0
                    && c.AllowedGrantTypes.Select(g => g.GrantType).All(GrantTypes.DeviceFlow.Contains)
                        => ClientTypes.Device,

                XpoClient c
                    when c.AllowedGrantTypes.Count > 0
                    && c.AllowedGrantTypes.Select(g => g.GrantType).All(GrantTypes.ClientCredentials.Contains)
                        => ClientTypes.Machine,

                XpoClient c
                    when c.AllowedGrantTypes.Count > 0
                    && c.AllowedGrantTypes.Select(g => g.GrantType).All(GrantTypes.ResourceOwnerPassword.Contains)
                        => ClientTypes.ResourceOwnerPassword,

                _ => null
            };

        //case ClientTypes.Empty:
        //    break;
        //case ClientTypes.Web:
        //    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Code, client));
        //    client.RequirePkce = true;
        //    client.RequireClientSecret = true;
        //    break;
        //case ClientTypes.Spa:
        //    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Code, client));
        //    client.RequirePkce = true;
        //    client.RequireClientSecret = false;
        //    break;
        //case ClientTypes.Native:
        //    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Code, client));
        //    client.RequirePkce = true;
        //    client.RequireClientSecret = false;
        //    break;
        //case ClientTypes.Machine:
        //    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.ClientCredentials, client));
        //    break;
        //case ClientTypes.Device:
        //    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.DeviceFlow, client));
        //    client.RequireClientSecret = false;
        //    client.AllowOfflineAccess = true;
        //    break;
        //default:
        //    throw new ArgumentOutOfRangeException();


    }
}
