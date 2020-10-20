
using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;

namespace Xenial.AspNetIdentity.Xpo.Mappers
{
    public class XPRoleMapperProfile : Profile
    {
        public XPRoleMapperProfile()
        {
            CreateMap<Models.XpoIdentityRole, IdentityRole>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityRoleClaim, IdentityRoleClaim<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityRoleClaim, Claim>()
                .ConstructUsing(c => new Claim(c.Type, c.Value))
                .ReverseMap()
                .ConstructUsingServiceLocator();
        }
    }
}
