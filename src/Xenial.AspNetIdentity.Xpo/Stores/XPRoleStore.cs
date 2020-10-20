using System;
using System.Collections.Generic;
using System.Data;
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
    public class XPRoleStore : XPRoleStore<IdentityRole, string, IdentityUserRole<string>, IdentityRoleClaim<string>,
        XpoIdentityRole, XpoIdentityUser, XpoIdentityRoleClaim>
    {
        public XPRoleStore(UnitOfWork unitOfWork, ILogger<XPRoleStore<IdentityRole, string, IdentityUserRole<string>, IdentityRoleClaim<string>, XpoIdentityRole, XpoIdentityUser, XpoIdentityRoleClaim>> logger, IdentityErrorDescriber describer) : base(unitOfWork, logger, describer)
        {
        }

        public XPRoleStore(UnitOfWork unitOfWork, ILogger<XPRoleStore<IdentityRole, string, IdentityUserRole<string>, IdentityRoleClaim<string>, XpoIdentityRole, XpoIdentityUser, XpoIdentityRoleClaim>> logger, IConfigurationProvider mapperConfiguration, IdentityErrorDescriber describer) : base(unitOfWork, logger, mapperConfiguration, describer)
        {
        }
    }

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

        protected virtual CriteriaOperator CreateRoleClaimOperator(string rolePropertyName, TKey roleKey)
            => new BinaryOperator(new OperandProperty(rolePropertyName), new OperandValue(roleKey), BinaryOperatorType.Equal);

        protected virtual CriteriaOperator CreateRoleClaimOperator(string rolePropertyName, TKey roleKey, string claimType, string claimValue)
           => new GroupOperator(GroupOperatorType.And,
               CreateRoleClaimOperator(rolePropertyName, roleKey),
               new BinaryOperator(new OperandProperty("Type"), new OperandValue(claimType), BinaryOperatorType.Equal),
               new BinaryOperator(new OperandProperty("Value"), new OperandValue(claimValue), BinaryOperatorType.Equal)
            );

        public async override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            var rolePropertyName = $"Role.{UnitOfWork.GetClassInfo(typeof(TXPRole)).KeyProperty}";
            var criteria = CreateRoleClaimOperator(rolePropertyName, role.Id);
            var collection = new XPCollection<TXPRoleClaim>(UnitOfWork, criteria);

            await collection.LoadAsync(cancellationToken);

            var mapper = MapperConfiguration.CreateMapper();
            var claims = collection
                .Select(c => mapper.Map<Claim>(c))
                .ToList();

            return claims;
        }

        public async override Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
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
            var rolePropertyName = $"Role.{UnitOfWork.GetClassInfo(typeof(TXPRole)).KeyProperty}";
            var criteria = CreateRoleClaimOperator(rolePropertyName, role.Id, claim.Type, claim.Value);
            var collection = new XPCollection<TXPRoleClaim>(UnitOfWork, criteria);

            await collection.LoadAsync(cancellationToken);
            await UnitOfWork.DeleteAsync(collection, cancellationToken);
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
