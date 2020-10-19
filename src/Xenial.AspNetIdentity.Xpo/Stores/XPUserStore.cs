using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Xenial.AspNetIdentity.Xpo.Mappers;
using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.AspNetIdentity.Xpo.Stores
{
    public class XPUserStore<TUser, TUserKey, TXPUser> :
         IQueryableUserStore<TUser>
        //,IUserPasswordStore<TUser>,
        //IUserSecurityStampStore<TUser>,
        //IUserEmailStore<TUser>,
        //IUserPhoneNumberStore<TUser>,
        //IUserLoginStore<TUser>,
        //IUserClaimStore<TUser>,
        //IUserRoleStore<TUser>,
        //IUserTwoFactorStore<TUser>,
        //IUserLockoutStore<TUser>,
        //IUserAuthenticationTokenStore<TUser>,
        //IUserAuthenticatorKeyStore<TUser>,
        //IUserTwoFactorRecoveryCodeStore<TUser>
        where TUser : IdentityUser<TUserKey>
        where TUserKey : IEquatable<TUserKey>
        where TXPUser : IXPObject
    {
        public Func<UnitOfWork> UnitOfWorkFactory { get; }
        public ILogger<XPUserStore<TUser, TUserKey, TXPUser>> Logger { get; }
        public MapperConfiguration MapperConfiguration { get; }
        private readonly Lazy<UnitOfWork> queryUnitOfWork;

        public XPUserStore(Func<UnitOfWork> unitOfWorkFactory, ILogger<XPUserStore<TUser, TUserKey, TXPUser>> logger)
        {
            UnitOfWorkFactory = unitOfWorkFactory;
            Logger = logger;
            MapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<XPUserMapperProfile>());
            queryUnitOfWork = new Lazy<UnitOfWork>(unitOfWorkFactory);
        }

        public IQueryable<TUser> Users => queryUnitOfWork.Value.Query<XpoIdentityUser>().ProjectTo<TUser>(MapperConfiguration);

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            try
            {
                using var uow = UnitOfWorkFactory();
                var mapper = MapperConfiguration.CreateMapper(t => uow.GetClassInfo(t).CreateObject(uow));
                var persistentUser = mapper.Map<TXPUser>(user);
                await uow.SaveAsync(persistentUser);
                await uow.CommitChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return HandleGenericException("create", ex);
            }
        }
        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            try
            {
                using var uow = UnitOfWorkFactory();
                var persistentUser = await uow.GetObjectByKeyAsync<TXPUser>(user.Id);
                if (persistentUser != null)
                {
                    await uow.DeleteAsync(persistentUser);
                    return IdentityResult.Success;
                }
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }
            catch (Exception ex)
            {
                return HandleGenericException("delete", ex);
            }
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Dispose()
        {
            if (queryUnitOfWork.IsValueCreated)
            {
                queryUnitOfWork.Value.Dispose();
            }
        }

        private IdentityResult HandleGenericException(string method, Exception ex)
        {
            var message = $"Failed to {method} the user.";
            Logger.LogError(ex, message);
            return IdentityResult.Failed(
                new IdentityError() { Description = message }
#if DEBUG
                    , new IdentityError() { Description = ex.Message }
#endif
                    );
        }
    }
}
