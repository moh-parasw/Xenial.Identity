using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MiddleTier;
using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Xenial.AspNetIdentity.Xpo.Mappers;

namespace Xenial.AspNetIdentity.Xpo.Stores
{
    public class XPRoleStore<
            TRole, TKey, TUserRole, TRoleClaim,
            TXPRole, TXPUser, TXPRoleClaim> :
            RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
        where TXPRole : IXPObject
        where TXPUser : IXPObject
        where TXPRoleClaim : IXPObject
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserRole : IdentityUserRole<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new()
    {
        public UnitOfWork UnitOfWork { get; }
        public ILogger<XPRoleStore<TRole, TKey, TUserRole, TRoleClaim, TXPRole, TXPUser, TXPRoleClaim>> Logger { get; }
        public IConfigurationProvider MapperConfiguration { get; }
        public XPRoleStore(UnitOfWork unitOfWork, ILogger<XPRoleStore<TRole, TKey, TUserRole, TRoleClaim, TXPRole, TXPUser, TXPRoleClaim>> logger, IConfigurationProvider mapperConfiguration, IdentityErrorDescriber describer) : base(describer)
        {
            UnitOfWork = unitOfWork;
            Logger = logger;
            MapperConfiguration = mapperConfiguration;
        }

        public XPRoleStore(UnitOfWork unitOfWork, ILogger<XPRoleStore<TRole, TKey, TUserRole, TRoleClaim, TXPRole, TXPUser, TXPRoleClaim>> logger, IdentityErrorDescriber describer)
            : this(unitOfWork, logger, new MapperConfiguration(opt => opt.AddProfile<XPRoleMapperProfile>()), describer) { }

        public override IQueryable<TRole> Roles
            => UnitOfWork
                .Query<TXPRole>()
                .ProjectTo<TRole>(MapperConfiguration);

        public async override Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            try
            {
                var mapper = MapperConfiguration.CreateMapper(t => UnitOfWork.GetClassInfo(t).CreateObject(UnitOfWork));
                var persistentRole = mapper.Map<TXPRole>(role);
                await UnitOfWork.SaveAsync(persistentRole, cancellationToken);
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

        public async override Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            try
            {
                var persistentRole = await UnitOfWork.GetObjectByKeyAsync<TXPRole>(role.Id, cancellationToken);
                if (persistentRole != null)
                {
                    await UnitOfWork.DeleteAsync(persistentRole, cancellationToken);
                    await UnitOfWork.CommitChangesAsync(cancellationToken);
                }
                return IdentityResult.Success;
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

        public async override Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            try
            {
                var persistentRole = await UnitOfWork.GetObjectByKeyAsync<TXPRole>(role.Id, cancellationToken);
                if (persistentRole == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = $"Role with Id: {role.Id} not found" });
                }
                var mapper = MapperConfiguration.CreateMapper();
                persistentRole = mapper.Map(role, persistentRole);
                await UnitOfWork.SaveAsync(persistentRole, cancellationToken);
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

        public async override Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var persistentRole = await UnitOfWork.GetObjectByKeyAsync<TXPRole>(ConvertIdFromString(id), cancellationToken);
            if (persistentRole == null)
            {
                return null;
            }

            var mapper = MapperConfiguration.CreateMapper();
            return mapper.Map<TRole>(persistentRole);
        }
        protected virtual CriteriaOperator CreateRoleOperator(string normalizedName)
            => new BinaryOperator("NormalizedName", normalizedName, BinaryOperatorType.Equal);

        public async override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var persistentRole = await UnitOfWork.FindObjectAsync<TXPRole>(CreateRoleOperator(normalizedName), cancellationToken);
            if (persistentRole == null)
            {
                return null;
            }

            var mapper = MapperConfiguration.CreateMapper();
            return mapper.Map<TRole>(persistentRole);
        }

        public async override Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            try
            {
                var roleInDb = await UnitOfWork.GetObjectByKeyAsync<TXPRole>(role.Id, cancellationToken);

                var claimClassInfo = UnitOfWork.GetClassInfo<TXPRoleClaim>();
                var claimInDb = (TXPRoleClaim)claimClassInfo.CreateNewObject(UnitOfWork);

                var mapper = MapperConfiguration.CreateMapper();
                claimInDb = mapper.Map(claim, claimInDb);

                claimClassInfo.KeyProperty.SetValue(claimInDb, Guid.NewGuid().ToString());
                claimClassInfo.FindMember("Role")?.SetValue(claimInDb, roleInDb);

                await UnitOfWork.SaveAsync(claimInDb);
            }
            catch (Exception ex)
            {
                HandleGenericException("add claim", ex);
            }
        }
        public override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

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
