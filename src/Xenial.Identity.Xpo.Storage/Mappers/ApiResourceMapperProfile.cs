using System.Collections.Generic;

using AutoMapper;

using Duende.IdentityServer.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Defines entity/model mapping for API resources.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class ApiResourceMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="ApiResourceMapperProfile"/>
        /// </summary>
        public ApiResourceMapperProfile()
        {
            _ = CreateMap<Models.XpoApiResourceProperty, KeyValuePair<string, string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoApiResource, ApiResource>(MemberList.Destination)
                .ConstructUsing(src => new ApiResource())
                .ForMember(x => x.ApiSecrets, opts => opts.MapFrom(x => x.Secrets))
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoApiResourceClaim, string>()
                .ConstructUsing(x => x.Type)
                .ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoApiResourceSecret, Secret>(MemberList.Destination)
                .ForMember(dest => dest.Type, opt => opt.Condition(srs => srs != null))
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoApiResourceScope, string>()
                .ConstructUsing(x => x.Scope)
                .ReverseMap()
                .ForMember(dest => dest.Scope, opt => opt.MapFrom(src => src))
                .ConstructUsingServiceLocator();
        }
    }
}
