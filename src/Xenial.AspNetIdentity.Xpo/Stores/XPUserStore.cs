using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Xenial.AspNetIdentity.Xpo.Mappers;
using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.AspNetIdentity.Xpo.Stores
{
    public class XPUserStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken, TXPUser> :
        UserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>,
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
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TXPUser : IXPObject
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
    {
        public Func<UnitOfWork> UnitOfWorkFactory { get; }
        public ILogger<XPUserStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken, TXPUser>> Logger { get; }
        public MapperConfiguration MapperConfiguration { get; }

        public XPUserStore(
            Func<UnitOfWork> unitOfWorkFactory,
            ILogger<XPUserStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken, TXPUser>> logger,
            IdentityErrorDescriber describer
        )
            : base(describer)
        {
            UnitOfWorkFactory = unitOfWorkFactory;
            Logger = logger;
            MapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<XPUserMapperProfile>());
        }

        public override IQueryable<TUser> Users
        {
            get
            {
                using var uow = UnitOfWorkFactory();
                return uow.Query<XpoIdentityUser>().ProjectTo<TUser>(MapperConfiguration);
            }
        }
        #region CRUD
        public async override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                using var uow = UnitOfWorkFactory();
                var mapper = MapperConfiguration.CreateMapper(t => uow.GetClassInfo(t).CreateObject(uow));
                var persistentUser = mapper.Map<TXPUser>(user);
                await uow.SaveAsync(persistentUser, cancellationToken);
                await uow.CommitChangesAsync(cancellationToken);
                return IdentityResult.Success;
            }
            catch (LockingException)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            catch (Exception ex)
            {
                return HandleGenericException("create", ex);
            }
        }

        public async override Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                using var uow = UnitOfWorkFactory();
                var persistentUser = await uow.GetObjectByKeyAsync<TXPUser>(user.Id, cancellationToken);
                if (persistentUser != null)
                {
                    await uow.DeleteAsync(persistentUser, cancellationToken);
                    await uow.CommitChangesAsync(cancellationToken);
                    return IdentityResult.Success;
                }
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }
            catch (LockingException)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            catch (Exception ex)
            {
                return HandleGenericException("delete", ex);
            }
        }

        public async override Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            try
            {
                using var uow = UnitOfWorkFactory();
                var existingObject = await uow.GetObjectByKeyAsync<TXPUser>(user.Id, cancellationToken);
                if (existingObject == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = $"User with Id: {user.Id} not found" });
                }
                var mapper = MapperConfiguration.CreateMapper();
                var persistentUser = mapper.Map(user, existingObject);
                await uow.SaveAsync(persistentUser, cancellationToken);
                await uow.CommitChangesAsync(cancellationToken);
                return IdentityResult.Success;
            }
            catch (LockingException)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            catch (Exception ex)
            {
                return HandleGenericException("update", ex);
            }
        }

        #endregion

        public override Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => FindUserAsync(ConvertIdFromString(userId), cancellationToken);

        protected async override Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                using var uow = UnitOfWorkFactory();
                var persistentUser = await uow.GetObjectByKeyAsync<TXPUser>(userId, cancellationToken);
                if (persistentUser != null)
                {
                    var mapper = MapperConfiguration.CreateMapper();
                    return mapper.Map<TUser>(persistentUser);
                }
            }
            catch (Exception ex)
            {
                HandleGenericException("find by Id", ex);
            }
            return null;
        }

        protected virtual CriteriaOperator CreateUserNameCriteria(string normalizedUserName) => new BinaryOperator("NormalizedUserName", normalizedUserName, BinaryOperatorType.Equal);

        public async override Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                using var uow = UnitOfWorkFactory();
                var persistentUser = await uow.FindObjectAsync<TXPUser>(CreateUserNameCriteria(normalizedUserName), cancellationToken);

                if (persistentUser != null)
                {
                    var mapper = MapperConfiguration.CreateMapper();
                    return mapper.Map<TUser>(persistentUser);
                }
            }
            catch (Exception ex)
            {
                HandleGenericException("find by Name", ex);
            }
            return null;
        }

        protected virtual CriteriaOperator CreateEmailCriteria(string normalizedEmail) => new BinaryOperator("NormalizedEmail", normalizedEmail, BinaryOperatorType.Equal);

        public async override Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                using var uow = UnitOfWorkFactory();
                var persistentUser = await uow.FindObjectAsync<TXPUser>(CreateEmailCriteria(normalizedEmail), cancellationToken);

                if (persistentUser != null)
                {
                    var mapper = MapperConfiguration.CreateMapper();
                    return mapper.Map<TUser>(persistentUser);
                }
            }
            catch (Exception ex)
            {
                HandleGenericException("find by Name", ex);
            }
            return null;
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

        protected override Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken) => throw new NotImplementedException();
        protected override Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) => throw new NotImplementedException();
        public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        protected override Task<TUserToken> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken) => throw new NotImplementedException();
        protected override Task AddUserTokenAsync(TUserToken token) => throw new NotImplementedException();
        protected override Task RemoveUserTokenAsync(TUserToken token) => throw new NotImplementedException();
    }
}
