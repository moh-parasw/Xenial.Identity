using System;
using System.Collections.Generic;
using System.Text;

using AutoMapper;

using Microsoft.AspNetCore.Identity;

namespace Xenial.AspNetIdentity.Xpo.Mappers
{
    public class XPUserMapperProfile : Profile
    {
        public XPUserMapperProfile()
        {
            CreateMap<Models.XpoIdentityUser, IdentityUser>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserClaim, IdentityUserClaim<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserLogin, IdentityUserLogin<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserToken, IdentityUserToken<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();
        }
    }
}
