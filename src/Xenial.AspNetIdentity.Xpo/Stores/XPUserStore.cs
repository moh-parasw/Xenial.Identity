using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DevExpress.Data.Filtering;
using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Xenial.AspNetIdentity.Xpo.Mappers;
using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.AspNetIdentity.Xpo.Stores
{
    public class XPUserStore<TUser, TKey, TXPUser> :
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
    {
        private bool disposed;
        public Func<UnitOfWork> UnitOfWorkFactory { get; }
        public ILogger<XPUserStore<TUser, TKey, TXPUser>> Logger { get; }
        public MapperConfiguration MapperConfiguration { get; }
        private readonly Lazy<UnitOfWork> queryUnitOfWork;

        public XPUserStore(Func<UnitOfWork> unitOfWorkFactory, ILogger<XPUserStore<TUser, TKey, TXPUser>> logger)
        {
            UnitOfWorkFactory = unitOfWorkFactory;
            Logger = logger;
            MapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<XPUserMapperProfile>());
            queryUnitOfWork = new Lazy<UnitOfWork>(unitOfWorkFactory);
        }

        public IQueryable<TUser> Users => queryUnitOfWork.Value.Query<XpoIdentityUser>().ProjectTo<TUser>(MapperConfiguration);

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
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
            catch (Exception ex)
            {
                return HandleGenericException("create", ex);
            }
        }
        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
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
            catch (Exception ex)
            {
                return HandleGenericException("delete", ex);
            }
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                using var uow = UnitOfWorkFactory();
                var persistentUser = await uow.GetObjectByKeyAsync<TXPUser>(ConvertIdFromString(userId), cancellationToken);
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

        protected virtual CriteriaOperator CreateUserNameCriteria(string userName) => new BinaryOperator("NormalizedUserName", userName, BinaryOperatorType.Equal);

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
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


        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
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
            catch (Exception ex)
            {
                return HandleGenericException("update", ex);
            }
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        /// Dispose the store
        /// </summary>
        public virtual void Dispose()
        {
            disposed = true;
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

        /// <summary>
        /// Converts the provided <paramref name="id"/> to a strongly typed key object.
        /// </summary>
        /// <param name="id">The id to convert.</param>
        /// <returns>An instance of <typeparamref name="TKey"/> representing the provided <paramref name="id"/>.</returns>
        public virtual TKey ConvertIdFromString(string id)
        {
            if (id == null)
            {
                return default(TKey);
            }
            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
        }

        /// <summary>
        /// Converts the provided <paramref name="id"/> to its string representation.
        /// </summary>
        /// <param name="id">The id to convert.</param>
        /// <returns>An <see cref="string"/> representation of the provided <paramref name="id"/>.</returns>
        public virtual string ConvertIdToString(TKey id)
        {
            if (object.Equals(id, default(TKey)))
            {
                return null;
            }
            return id.ToString();
        }

    }
}
