using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class XPUserStore<
            TUser, TKey, TUserClaim, TUserLogin, TUserToken,
            TXPUser, TXPUserLogin, TXPUserToken> :
        UserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>,
        IQueryableUserStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        //IUserRoleStore<TUser>,
        IUserTwoFactorStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserAuthenticationTokenStore<TUser>,
        IUserAuthenticatorKeyStore<TUser>,
        IUserTwoFactorRecoveryCodeStore<TUser>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TXPUser : IXPObject
        where TXPUserLogin : IXPObject
        where TXPUserToken : IXPObject
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
    {
        public UnitOfWork UnitOfWork { get; }
        public ILogger<
            XPUserStore<
                TUser, TKey, TUserClaim, TUserLogin, TUserToken,
                TXPUser, TXPUserLogin, TXPUserToken
            >
        > Logger
        { get; }

        public IConfigurationProvider MapperConfiguration { get; }

        public XPUserStore(
            UnitOfWork unitOfWork,
            ILogger<
                XPUserStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken,
                TXPUser, TXPUserLogin, TXPUserToken
                >
            > logger,
            IdentityErrorDescriber describer
        )
            : this(unitOfWork, logger, describer, new MapperConfiguration(cfg => cfg.AddProfile<XPUserMapperProfile>()))
        {
        }

        public XPUserStore(
            UnitOfWork unitOfWork,
            ILogger<
                XPUserStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken,
                    TXPUser, TXPUserLogin, TXPUserToken
                >
            > logger,
            IdentityErrorDescriber describer,
            IConfigurationProvider configurationProvider
        )
            : base(describer)
        {
            UnitOfWork = unitOfWork;
            Logger = logger;
            MapperConfiguration = configurationProvider;
        }

        public override IQueryable<TUser> Users
            => UnitOfWork
                .Query<XpoIdentityUser>()
                .ProjectTo<TUser>(MapperConfiguration);

        #region CRUD
        public async override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                var mapper = MapperConfiguration.CreateMapper(t => UnitOfWork.GetClassInfo(t).CreateObject(UnitOfWork));
                var persistentUser = mapper.Map<TXPUser>(user);
                await UnitOfWork.SaveAsync(persistentUser, cancellationToken);
                await UnitOfWork.CommitChangesAsync(cancellationToken);
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
                var persistentUser = await UnitOfWork.GetObjectByKeyAsync<TXPUser>(user.Id, cancellationToken);
                if (persistentUser != null)
                {
                    await UnitOfWork.DeleteAsync(persistentUser, cancellationToken);
                    await UnitOfWork.CommitChangesAsync(cancellationToken);
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
                var existingObject = await UnitOfWork.GetObjectByKeyAsync<TXPUser>(user.Id, cancellationToken);
                if (existingObject == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = $"User with Id: {user.Id} not found" });
                }
                var mapper = MapperConfiguration.CreateMapper();
                var persistentUser = mapper.Map(user, existingObject);
                await UnitOfWork.SaveAsync(persistentUser, cancellationToken);
                await UnitOfWork.CommitChangesAsync(cancellationToken);
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

        #region Query

        public override Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => FindUserAsync(ConvertIdFromString(userId), cancellationToken);

        protected async override Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                var persistentUser = await UnitOfWork.GetObjectByKeyAsync<TXPUser>(userId, cancellationToken);
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
                var persistentUser = await UnitOfWork.FindObjectAsync<TXPUser>(CreateUserNameCriteria(normalizedUserName), cancellationToken);

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
                var persistentUser = await UnitOfWork.FindObjectAsync<TXPUser>(CreateEmailCriteria(normalizedEmail), cancellationToken);

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

        #endregion

        #region Logins

        public async override Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            var persistentUser = await UnitOfWork.GetObjectByKeyAsync<TXPUser>(user.Id, cancellationToken);
            if (persistentUser == null)
            {
                throw new ArgumentNullException(nameof(persistentUser));
            }

            var persistentLogin = (TXPUserLogin)UnitOfWork.GetClassInfo<TXPUserLogin>().CreateNewObject(UnitOfWork);
            var mapper = MapperConfiguration.CreateMapper();
            persistentLogin = mapper.Map(login, persistentLogin);

            UnitOfWork.GetClassInfo<TXPUserLogin>().FindMember("Id")?.SetValue(persistentLogin, Guid.NewGuid().ToString());
            UnitOfWork.GetClassInfo<TXPUserLogin>().FindMember("User")?.SetValue(persistentLogin, persistentUser);

            await UnitOfWork.SaveAsync(persistentLogin, cancellationToken);
        }
        protected override Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken) => throw new NotImplementedException();

        protected virtual CriteriaOperator CreateLoginCriteria(string loginProvider, string providerKey)
            => new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(new OperandProperty("LoginProvider"), new OperandValue(loginProvider), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("ProviderKey"), new OperandValue(providerKey), BinaryOperatorType.Equal)
            );

        protected virtual CriteriaOperator CreateLoginCriteria(string userPropertyName, TKey userKey, string loginProvider, string providerKey)
            => new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(new OperandProperty(userPropertyName), new OperandValue(userKey), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("LoginProvider"), new OperandValue(loginProvider), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("ProviderKey"), new OperandValue(providerKey), BinaryOperatorType.Equal)
            );

        protected async override Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var userLogin = await UnitOfWork.FindObjectAsync<TXPUserLogin>(CreateLoginCriteria(loginProvider, providerKey), cancellationToken);
            if (userLogin != null)
            {
                var mapper = MapperConfiguration.CreateMapper();
                return mapper.Map<TUserLogin>(userLogin);
            }
            return null;
        }

        public async override Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var userLogin = await UnitOfWork.FindObjectAsync<TXPUserLogin>(CreateLoginCriteria(userPropertyName, user.Id, loginProvider, providerKey), cancellationToken);

            if (userLogin != null)
            {
                await UnitOfWork.DeleteAsync(userLogin, cancellationToken);
            }
        }

        public override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default) => throw new NotImplementedException();


        ///// <summary>
        ///// Called to create a new instance of a <see cref="IdentityUserLogin{TKey}"/>.
        ///// </summary>
        ///// <param name="user">The associated user.</param>
        ///// <param name="login">The sasociated login.</param>
        ///// <returns></returns>
        //protected virtual TUserLogin CreateUserLogin(TXPUser user, UserLoginInfo login)
        //{
        //    return new TUserLogin
        //    {
        //        UserId = user.Id,
        //        ProviderKey = login.ProviderKey,
        //        LoginProvider = login.LoginProvider,
        //        ProviderDisplayName = login.ProviderDisplayName
        //    };
        //}

        #endregion

        public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();


        #region Tokens

        protected virtual CriteriaOperator CreateTokenCriteria(string userPropertyName, TKey userKey, string loginProvider, string name)
            => new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(new OperandProperty(userPropertyName), new OperandValue(userKey), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("LoginProvider"), new OperandValue(loginProvider), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("Name"), new OperandValue(name), BinaryOperatorType.Equal)
            );

        protected async override Task<TUserToken> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var token = await UnitOfWork.FindObjectAsync<TXPUserToken>(CreateTokenCriteria(userPropertyName, user.Id, loginProvider, name), cancellationToken);
            if (token != null)
            {
                var mapper = MapperConfiguration.CreateMapper();
                return mapper.Map<TUserToken>(token);
            }

            return null;
        }

        protected async override Task AddUserTokenAsync(TUserToken token)
        {
            var user = await UnitOfWork.GetObjectByKeyAsync<TXPUser>(token.UserId);
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var tokenClassInfo = UnitOfWork.GetClassInfo(typeof(TXPUserToken));
            var persistentToken = (TXPUserToken)tokenClassInfo.CreateNewObject(UnitOfWork);

            var mapper = MapperConfiguration.CreateMapper();
            persistentToken = mapper.Map(token, persistentToken);
            tokenClassInfo.FindMember("User")?.SetValue(persistentToken, user);
            tokenClassInfo.FindMember("Id")?.SetValue(persistentToken, Guid.NewGuid().ToString());

            await UnitOfWork.SaveAsync(persistentToken);
        }

        protected async override Task RemoveUserTokenAsync(TUserToken token)
        {
            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var persistentToken = await UnitOfWork.FindObjectAsync<TXPUserToken>(CreateTokenCriteria(userPropertyName, token.UserId, token.LoginProvider, token.Name));
            if (persistentToken != null)
            {
                await UnitOfWork.DeleteAsync(persistentToken);
            }
        }

        /// <summary>
        /// Sets the token value for a particular user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="loginProvider">The authentication provider for the token.</param>
        /// <param name="name">The name of the token.</param>
        /// <param name="value">The value of the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public override async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var token = await FindTokenAsync(user, loginProvider, name, cancellationToken);
            if (token == null)
            {
                await AddUserTokenAsync(CreateUserToken(user, loginProvider, name, value));
            }
            else
            {
                await UpdateUserTokenAsync(CreateUserToken(user, loginProvider, name, value));
            }
        }

        protected async virtual Task UpdateUserTokenAsync(TUserToken token)
        {
            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var persistentToken = await UnitOfWork.FindObjectAsync<TXPUserToken>(CreateTokenCriteria(userPropertyName, token.UserId, token.LoginProvider, token.Name));
            if (persistentToken != null)
            {
                var mapper = MapperConfiguration.CreateMapper();
                mapper.Map(token, persistentToken);
                await UnitOfWork.SaveAsync(persistentToken);
            }
        }

        #endregion

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
