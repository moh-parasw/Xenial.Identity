
using System;
using System.Security.Claims;

using AutoMapper;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;

namespace Xenial.AspNetIdentity.Xpo.Mappers
{
    public class XPIdentityMapperProfile : XPIdentityMapperProfile<
        IdentityUser, IdentityRole,
        Models.XpoIdentityUser, Models.XpoIdentityRole
    >
    { }

    public class XPIdentityMapperProfile<TUser, TRole, TXPUser, TXPRole> : Profile
        where TUser : IdentityUser
        where TRole : IdentityRole
        where TXPUser : IXPObject
        where TXPRole : IXPObject
    {
        public XPIdentityMapperProfile()
        {
            CreateMap<TXPUser, TUser>()
                //TODO: Convert from DateTimeOffset to DateTime and via versa
                .ForMember(o => o.LockoutEnd, o => o.Ignore())
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserClaim, IdentityUserClaim<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            CreateMap<Models.XpoIdentityUserClaim, Claim>()
                .ConstructUsing(c => new Claim(c.Type, c.Value))
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

            CreateMap<TXPRole, TRole>()
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
