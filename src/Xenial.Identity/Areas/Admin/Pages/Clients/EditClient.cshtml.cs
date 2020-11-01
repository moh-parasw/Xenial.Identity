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
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Schema;
using IdentityServer4.Stores;

namespace Xenial.Identity.Areas.Admin.Pages.Clients
{
    public class EditClientModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddClientModel> logger;
        private readonly IResourceStore resourceStore;
        public EditClientModel(UnitOfWork unitOfWork, ILogger<AddClientModel> logger, IResourceStore resourceStore)
            => (this.unitOfWork, this.logger, this.resourceStore) = (unitOfWork, logger, resourceStore);

        public class ClientInputModel
        {
            [Required]
            public string ClientId { get; set; }
            [Required]
            public string ClientName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public bool EnableLocalLogin { get; set; }

            #region Configuration

            public string ProtocolType { get; set; }
            public bool RequireClientSecret { get; set; }
            public bool RequirePkce { get; set; }
            public bool AllowPlainTextPkce { get; set; }
            public bool AllowOfflineAccess { get; set; }
            public bool AllowAccessTokensViaBrowser { get; set; }

            public string AllowedScopes { get; set; }
            public string RedirectUris { get; set; }
            public string AllowedGrantTypes { get; set; }
            public string AllowedCorsOrigins { get; set; }

            #endregion

            #region Token

            public int IdentityTokenLifetime { get; set; }
            public int AccessTokenLifetime { get; set; }
            public AccessTokenType AccessTokenType { get; set; }
            public int AuthorizationCodeLifetime { get; set; }
            public int AbsoluteRefreshTokenLifetime { get; set; }
            public int SlidingRefreshTokenLifetime { get; set; }
            public TokenUsage RefreshTokenUsage { get; set; }
            public TokenExpiration RefreshTokenExpiration { get; set; }

            public bool UpdateAccessTokenClaimsOnRefresh { get; set; }
            public bool IncludeJwtId { get; set; }
            public bool AlwaysSendClientClaims { get; set; }
            public bool AlwaysIncludeUserClaimsInIdToken { get; set; }

            public string ClientClaimsPrefix { get; set; }
            public string PairWiseSubjectSalt { get; set; }

            #endregion

            #region Consent

            public bool RequireConsent { get; set; }
            public bool AllowRememberConsent { get; set; }
            public string ClientUri { get; set; }
            public string LogoUri { get; set; }

            #endregion

            #region Device Flow

            public string UserCodeType { get; set; }
            public int DeviceCodeLifetime { get; set; }

            #endregion
        }

        public string AllowedGrantTypes { get; } = string.Join(",",
            GrantTypes.ClientCredentials
                .Concat(GrantTypes.Code)
                .Concat(GrantTypes.CodeAndClientCredentials)
                .Concat(GrantTypes.DeviceFlow)
                .Concat(GrantTypes.Hybrid)
                .Concat(GrantTypes.HybridAndClientCredentials)
                .Concat(GrantTypes.Implicit)
                .Concat(GrantTypes.ImplicitAndClientCredentials)
                .Concat(GrantTypes.ResourceOwnerPassword)
                .Concat(GrantTypes.ResourceOwnerPasswordAndClientCredentials)
                .Distinct()
            );

        public string AllowedScopes { get; set; }

        public SelectList AccessTokenTypes { get; } = new SelectList(Enum.GetValues(typeof(AccessTokenType)));
        public SelectList RefreshTokenUsages { get; } = new SelectList(Enum.GetValues(typeof(TokenUsage)));
        public SelectList RefreshTokenExpirations { get; } = new SelectList(Enum.GetValues(typeof(TokenExpiration)));

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
                => CreateMap<XpoClient, ClientInputModel>()
                    .ForMember(m => m.AllowedScopes, o => o.Ignore())
                    .ForMember(m => m.RedirectUris, o => o.Ignore())
                    .ForMember(m => m.AllowedGrantTypes, o => o.Ignore())
                    .ForMember(m => m.AllowedCorsOrigins, o => o.Ignore())
                    .ReverseMap()
                    .ForMember(m => m.AllowedScopes, o => o.Ignore())
                    .ForMember(m => m.RedirectUris, o => o.Ignore())
                    .ForMember(m => m.AllowedGrantTypes, o => o.Ignore())
                    .ForMember(m => m.AllowedCorsOrigins, o => o.Ignore())
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

            await FetchAllowedScopes();

            ClientType = clientType.HasValue ? clientType.Value : GuessClientType(client);
            Input = Mapper.Map(client, Input);
            Input.AllowedGrantTypes = string.Join(",", client.AllowedGrantTypes.Select(s => s.GrantType));
            Input.AllowedScopes = string.Join(",", client.AllowedScopes.Select(s => s.Scope));
            Input.RedirectUris = string.Join(",", client.RedirectUris.Select(s => s.RedirectUri));
            Input.AllowedCorsOrigins = string.Join(",", client.AllowedCorsOrigins.Select(s => s.Origin));

            return Page();
        }

        private async Task FetchAllowedScopes()
        {
            var resources = await resourceStore.GetAllResourcesAsync();
            AllowedScopes = string.Join(",", resources.ToScopeNames().Distinct());
        }

        internal class Tag
        {
            public string Value { get; set; }
        }
        public async Task<IActionResult> OnPost([FromRoute] int id, [FromQuery] ClientTypes? clientType = null)
        {
            Id = id;
            await FetchAllowedScopes();
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

                    await MapAllowedGrantTypes(client);
                    await MapAllowedScopes(client);
                    await MapRedirectUris(client);
                    await MapAllowedCorsOrigins(client);

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

        private async Task MapAllowedCorsOrigins(XpoClient client)
        {
            if (!string.IsNullOrEmpty(Input.AllowedCorsOrigins))
            {
                var jsonArray = Newtonsoft.Json.Linq.JArray.Parse(Input.AllowedCorsOrigins);
                var values = jsonArray.Select(j => j.ToObject<Tag>()).ToList();

                await ClearAllowedCorsOrigins(client);

                client.AllowedCorsOrigins.AddRange(values.Select(allowedCorsOrigin => new XpoClientCorsOrigin(unitOfWork)
                {
                    Origin = allowedCorsOrigin.Value
                }));
            }
            else
            {
                await ClearAllowedCorsOrigins(client);
            }
            async Task ClearAllowedCorsOrigins(XpoClient client)
            {
                foreach (var allowedCorsOrigin in client.AllowedCorsOrigins.ToList())
                {
                    await unitOfWork.DeleteAsync(allowedCorsOrigin);
                }
            }
        }

        private async Task MapRedirectUris(XpoClient client)
        {
            if (!string.IsNullOrEmpty(Input.RedirectUris))
            {
                var jsonArray = Newtonsoft.Json.Linq.JArray.Parse(Input.RedirectUris);
                var values = jsonArray.Select(j => j.ToObject<Tag>()).ToList();

                await ClearRedirectUris(client);

                client.RedirectUris.AddRange(values.Select(redirectUri => new XpoClientRedirectUri(unitOfWork)
                {
                    RedirectUri = redirectUri.Value
                }));
            }
            else
            {
                await ClearRedirectUris(client);
            }
            async Task ClearRedirectUris(XpoClient client)
            {
                foreach (var redirectUri in client.RedirectUris.ToList())
                {
                    await unitOfWork.DeleteAsync(redirectUri);
                }
            }
        }

        private async Task MapAllowedScopes(XpoClient client)
        {
            if (!string.IsNullOrEmpty(Input.AllowedScopes))
            {
                var jsonArray = Newtonsoft.Json.Linq.JArray.Parse(Input.AllowedScopes);
                var values = jsonArray.Select(j => j.ToObject<Tag>()).ToList();
                await ClearAllowedScopes(client);

                client.AllowedScopes.AddRange(values.Select(scope => new XpoClientScope(unitOfWork)
                {
                    Scope = scope.Value
                }));
            }
            else
            {
                await ClearAllowedScopes(client);
            }
            async Task ClearAllowedScopes(XpoClient client)
            {
                foreach (var allowedScope in client.AllowedScopes.ToList())
                {
                    await unitOfWork.DeleteAsync(allowedScope);
                }
            }
        }

        private async Task MapAllowedGrantTypes(XpoClient client)
        {
            if (!string.IsNullOrEmpty(Input.AllowedGrantTypes))
            {
                var jsonArray = Newtonsoft.Json.Linq.JArray.Parse(Input.AllowedGrantTypes);
                var values = jsonArray.Select(j => j.ToObject<Tag>()).ToList();

                await ClearAllowedGrantTypes(client);

                client.AllowedGrantTypes.AddRange(values.Select(grant => new XpoClientGrantType(unitOfWork)
                {
                    GrantType = grant.Value
                }));
            }
            else
            {
                await ClearAllowedGrantTypes(client);
            }
            async Task ClearAllowedGrantTypes(XpoClient client)
            {
                foreach (var allowedGrantType in client.AllowedGrantTypes.ToList())
                {
                    await unitOfWork.DeleteAsync(allowedGrantType);
                }
            }
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
    }
}
