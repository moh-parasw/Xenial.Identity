using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DevExpress.ExpressApp.MiddleTier;
using DevExpress.Xpo;

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

        public override Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
