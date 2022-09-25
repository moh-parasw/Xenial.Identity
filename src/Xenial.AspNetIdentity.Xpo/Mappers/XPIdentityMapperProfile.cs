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
            _ = CreateMap<TXPUser, TUser>()
                //TODO: Convert from DateTimeOffset to DateTime and via versa
                .ForMember(o => o.LockoutEnd, o => o.Ignore())
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoIdentityUserClaim, IdentityUserClaim<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoIdentityUserClaim, Claim>()
                .ConstructUsing(c => new Claim(c.Type, c.Value))
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoIdentityUserLogin, IdentityUserLogin<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoIdentityUserToken, IdentityUserToken<string>>()
                .ReverseMap()
                .ForMember(m => m.User, o => o.Ignore())
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoIdentityUserLogin, UserLoginInfo>()
                .ConstructUsing(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
                .ReverseMap()
                .ForMember(m => m.User, o => o.Ignore())
                .ConstructUsingServiceLocator();

            _ = CreateMap<TXPRole, TRole>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoIdentityRoleClaim, IdentityRoleClaim<string>>()
                .ReverseMap()
                .ConstructUsingServiceLocator();

            _ = CreateMap<Models.XpoIdentityRoleClaim, Claim>()
                .ConstructUsing(c => new Claim(c.Type, c.Value))
                .ReverseMap()
                .ConstructUsingServiceLocator();
        }
    }
}
