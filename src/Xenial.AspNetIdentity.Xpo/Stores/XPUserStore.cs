using System;
using System.Collections.Generic;
using System.Globalization;
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
            TXPUser, TXPRole, TXPUserClaim, TXPUserLogin, TXPUserToken> :
        UserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>,
        IQueryableUserStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserTwoFactorStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserAuthenticationTokenStore<TUser>,
        IUserAuthenticatorKeyStore<TUser>,
        IUserTwoFactorRecoveryCodeStore<TUser>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TXPUser : IXPObject
        where TXPUserClaim : IXPObject
        where TXPRole : IXPObject
        where TXPUserLogin : IXPObject
        where TXPUserToken : IXPObject
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
    {
        public UnitOfWork UnitOfWork { get; }
        public ILogger<XPUserStore<
                TUser, TKey, TUserClaim, TUserLogin, TUserToken,
                TXPUser, TXPRole, TXPUserClaim, TXPUserLogin, TXPUserToken>
        > Logger
        { get; }

        public IConfigurationProvider MapperConfiguration { get; }

        public XPUserStore(
            UnitOfWork unitOfWork,
            ILogger<
                XPUserStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken,
                TXPUser, TXPRole, TXPUserClaim, TXPUserLogin, TXPUserToken
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
                    TXPUser, TXPRole, TXPUserClaim, TXPUserLogin, TXPUserToken
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

        protected virtual CriteriaOperator CreateUserNameCriteria(string normalizedUserName)
            => new BinaryOperator("NormalizedUserName", normalizedUserName, BinaryOperatorType.Equal);

        public async override Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            try
            {
                var persistentUser = await UnitOfWork.FindObjectAsync<TXPUser>(
                    CreateUserNameCriteria(normalizedUserName),
                    cancellationToken
                );

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

        protected virtual CriteriaOperator CreateEmailCriteria(string normalizedEmail)
            => new BinaryOperator("NormalizedEmail", normalizedEmail, BinaryOperatorType.Equal);

        public async override Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            try
            {
                var persistentUser = await UnitOfWork.FindObjectAsync<TXPUser>(
                    CreateEmailCriteria(normalizedEmail),
                    cancellationToken
                );

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

        public async override Task AddLoginAsync(
            TUser user,
            UserLoginInfo login,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

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

        protected async override Task<TUserLogin> FindUserLoginAsync(
            TKey userId,
            string loginProvider,
            string providerKey,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var userLogin = await UnitOfWork.FindObjectAsync<TXPUserLogin>(
                CreateLoginCriteria(userPropertyName, userId, loginProvider, providerKey),
                cancellationToken
            );
            if (userLogin != null)
            {
                var mapper = MapperConfiguration.CreateMapper();
                return mapper.Map<TUserLogin>(userLogin);
            }
            return null;
        }

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

        protected virtual CriteriaOperator CreateLoginCriteria(string userPropertyName, TKey userKey)
            => new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(new OperandProperty(userPropertyName), new OperandValue(userKey), BinaryOperatorType.Equal)
            );

        protected async override Task<TUserLogin> FindUserLoginAsync(
            string loginProvider,
            string providerKey,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var userLogin = await UnitOfWork.FindObjectAsync<TXPUserLogin>(
                CreateLoginCriteria(loginProvider, providerKey),
                cancellationToken
            );
            if (userLogin != null)
            {
                var mapper = MapperConfiguration.CreateMapper();
                return mapper.Map<TUserLogin>(userLogin);
            }
            return null;
        }

        public async override Task RemoveLoginAsync(
            TUser user,
            string loginProvider,
            string providerKey,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var userLogin = await UnitOfWork.FindObjectAsync<TXPUserLogin>(
                CreateLoginCriteria(userPropertyName, user.Id, loginProvider, providerKey),
                cancellationToken
            );

            if (userLogin != null)
            {
                await UnitOfWork.DeleteAsync(userLogin, cancellationToken);
            }
        }

        public async override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var collection = new XPCollection<TXPUserLogin>(
                UnitOfWork,
                CreateLoginCriteria(userPropertyName, user.Id)
            );
            await collection.LoadAsync(cancellationToken);

            var mapper = MapperConfiguration.CreateMapper();
            var loginInfos = collection
                .Select(l => mapper.Map<UserLoginInfo>(l))
                .ToList();
            return loginInfos;
        }

        #endregion

        #region Claims

        public async override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var persistentUser = await UnitOfWork.GetObjectByKeyAsync<TXPUser>(user.Id, cancellationToken);

            var mapper = MapperConfiguration.CreateMapper();
            foreach (var claim in claims)
            {
                var persistentClaim = (TXPUserClaim)UnitOfWork.GetClassInfo<TXPUserClaim>().CreateNewObject(UnitOfWork);
                persistentClaim = mapper.Map(claim, persistentClaim);

                UnitOfWork.GetClassInfo<TXPUserClaim>().KeyProperty.SetValue(persistentClaim, Guid.NewGuid().ToString());
                UnitOfWork.GetClassInfo<TXPUserClaim>().FindMember("User")?.SetValue(persistentClaim, persistentUser);
            }
        }
        public async override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var collection = new XPCollection<TXPUserClaim>(UnitOfWork, CreateClaimsCriteria(userPropertyName, user.Id));
            await collection.LoadAsync(cancellationToken);

            var mapper = MapperConfiguration.CreateMapper();
            var claims = collection.Select(c => mapper.Map<Claim>(c)).ToList();
            return claims;
        }

        protected virtual CriteriaOperator CreateClaimsCriteria(string userPropertyName, TKey userKey)
           => new GroupOperator(GroupOperatorType.And,
               new BinaryOperator(new OperandProperty(userPropertyName), new OperandValue(userKey), BinaryOperatorType.Equal)
           );

        protected virtual CriteriaOperator CreateClaimsCriteria(string userPropertyName, TKey userKey, string claimType, string claimValue)
            => new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(new OperandProperty(userPropertyName), new OperandValue(userKey), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("Type"), new OperandValue(claimType), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("Value"), new OperandValue(claimValue), BinaryOperatorType.Equal)
            );
        protected virtual CriteriaOperator CreateClaimsCriteria(string claimType, string claimValue)
            => new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(new OperandProperty("Type"), new OperandValue(claimType), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("Value"), new OperandValue(claimValue), BinaryOperatorType.Equal)
            );

        public async override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            if (newClaim == null)
            {
                throw new ArgumentNullException(nameof(newClaim));
            }

            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var claimToReplace = await UnitOfWork.FindObjectAsync<TXPUserClaim>(
                CreateClaimsCriteria(userPropertyName, user.Id, claim.Type, claim.Value),
                cancellationToken
            );

            if (claimToReplace != null)
            {
                UnitOfWork.GetClassInfo<TXPUserClaim>().FindMember("Type")?.SetValue(claimToReplace, newClaim.Type);
                UnitOfWork.GetClassInfo<TXPUserClaim>().FindMember("Value")?.SetValue(claimToReplace, newClaim.Value);
            }
        }

        public async override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";

            foreach (var claim in claims)
            {
                var collection = new XPCollection<TXPUserClaim>(UnitOfWork, CreateClaimsCriteria(userPropertyName, user.Id, claim.Type, claim.Value));
                await collection.LoadAsync(cancellationToken);
                await UnitOfWork.DeleteAsync(collection, cancellationToken);
            }
        }

        public async override Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var collection = new XPCollection<TXPUserClaim>(UnitOfWork, CreateClaimsCriteria(claim.Type, claim.Value));
            await collection.LoadAsync(cancellationToken);
            var memberInfo = UnitOfWork.GetClassInfo<TXPUserClaim>().FindMember("User");
            var mapper = MapperConfiguration.CreateMapper();

            var users = collection
                .Select(c => memberInfo.GetValue(c))
                .Select(u => mapper.Map<TUser>(u))
                .ToList();

            return users;
        }

        #endregion

        #region Tokens

        protected virtual CriteriaOperator CreateTokenCriteria(string userPropertyName, TKey userKey, string loginProvider, string name)
            => new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(new OperandProperty(userPropertyName), new OperandValue(userKey), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("LoginProvider"), new OperandValue(loginProvider), BinaryOperatorType.Equal),
                new BinaryOperator(new OperandProperty("Name"), new OperandValue(name), BinaryOperatorType.Equal)
            );

        protected async override Task<TUserToken> FindTokenAsync(
            TUser user,
            string loginProvider,
            string name,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userPropertyName = $"User.{UnitOfWork.GetClassInfo(typeof(TXPUser)).KeyProperty}";
            var token = await UnitOfWork.FindObjectAsync<TXPUserToken>(
                CreateTokenCriteria(userPropertyName, user.Id, loginProvider, name),
                cancellationToken
            );
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
            var persistentToken = await UnitOfWork.FindObjectAsync<TXPUserToken>(
                CreateTokenCriteria(userPropertyName, token.UserId, token.LoginProvider, token.Name)
            );
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
        public override async Task SetTokenAsync(
            TUser user,
            string loginProvider,
            string name,
            string value,
            CancellationToken cancellationToken
        )
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

        #region Roles

        protected virtual CriteriaOperator CreateRoleCriteria(string normalizedRoleName)
            => new BinaryOperator("NormalizedName", normalizedRoleName, BinaryOperatorType.Equal);

        public async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var role = await UnitOfWork.FindObjectAsync<TXPRole>(CreateRoleCriteria(normalizedRoleName), cancellationToken);
            if (role == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist.", normalizedRoleName));
            }
            var userInDb = await UnitOfWork.GetObjectByKeyAsync<TXPUser>(user.Id, cancellationToken);
            var rolesInDb = UnitOfWork.GetClassInfo<TXPUser>().FindMember("Roles")?.GetValue(userInDb);

            if (rolesInDb is IList<TXPRole> roles)
            {
                roles.Add(role);
            }
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();

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
