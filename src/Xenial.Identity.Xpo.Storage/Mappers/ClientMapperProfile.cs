using System.Collections.Generic;
using System.Security.Claims;

using AutoMapper;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Defines entity/model mapping for clients.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class ClientMapperProfile : Profile
    {
        /// <summary>
        /// <see>
        ///     <cref>{ClientMapperProfile}</cref>
        /// </see>
        /// </summary>
        public ClientMapperProfile()
        {
            _ = CreateMap<XpoClientProperty, KeyValuePair<string, string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClient, Client>()
                .ForMember(dest => dest.ProtocolType, opt => opt.Condition(srs => srs != null))
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientCorsOrigin, string>()
                .ConstructUsing(src => src.Origin)
                .ReverseMap()
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientIdPRestriction, string>()
                .ConstructUsing(src => src.Provider)
                .ReverseMap()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientClaim, ClientClaim>(MemberList.None)
                .ConstructUsing(src => new ClientClaim(src.Type, src.Value, ClaimValueTypes.String))
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientScope, string>()
                .ConstructUsing(src => src.Scope)
                .ReverseMap()
                .ForMember(dest => dest.Scope, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientPostLogoutRedirectUri, string>()
                .ConstructUsing(src => src.PostLogoutRedirectUri)
                .ReverseMap()
                .ForMember(dest => dest.PostLogoutRedirectUri, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientRedirectUri, string>()
                .ConstructUsing(src => src.RedirectUri)
                .ReverseMap()
                .ForMember(dest => dest.RedirectUri, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientGrantType, string>()
                .ConstructUsing(src => src.GrantType)
                .ReverseMap()
                .ForMember(dest => dest.GrantType, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();

            _ = CreateMap<XpoClientSecret, Secret>(MemberList.Destination)
                .ForMember(dest => dest.Type, opt => opt.Condition(srs => srs != null))
                .ReverseMap()
                .ConstructUsingServiceLocator();
        }
    }
}
