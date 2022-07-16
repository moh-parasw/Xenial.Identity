using System.Collections.Generic;

using AutoMapper;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Defines entity/model mapping for scopes.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class ScopeMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="ScopeMapperProfile"/>
        /// </summary>
        public ScopeMapperProfile()
        {
            CreateMap<XpoApiScopeProperty, KeyValuePair<string, string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<XpoApiScopeClaim, string>()
               .ConstructUsing(x => x.Type)
               .ReverseMap()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src))
               .ConstructUsingServiceLocator();

            CreateMap<XpoApiScope, ApiScope>(MemberList.Destination)
                .ConstructUsing(src => new ApiScope())
                .ForMember(x => x.Properties, opts => opts.MapFrom(x => x.Properties))
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(x => x.UserClaims))
                .ReverseMap()
                .ConstructUsingServiceLocator();
        }
    }
}
