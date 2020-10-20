﻿
using System.Security.Claims;

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

            CreateMap<Models.XpoIdentityUserClaim, Claim>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserLogin, IdentityUserLogin<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserToken, IdentityUserToken<string>>()
                .ReverseMap()
                .ForMember(m => m.User, o => o.Ignore())
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserLogin, UserLoginInfo>()
                .ConstructUsing(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
                .ReverseMap()
                .ForMember(m => m.User, o => o.Ignore())
                .ConstructUsingServiceLocator();
        }
    }
}
