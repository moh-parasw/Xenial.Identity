
using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;

namespace Xenial.AspNetIdentity.Xpo.Mappers
{
    public class XPUserMapperProfile : XPUserMapperProfile<IdentityUser> { }

    public class XPUserMapperProfile<TUser> : Profile
        where TUser : IdentityUser
    {
        public XPUserMapperProfile()
        {
            CreateMap<Models.XpoIdentityUser, TUser>()
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
        }
    }
}
