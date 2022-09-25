using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Stores
{
    /// <summary>
    /// Implementation of IPersistedGrantStore thats uses Xpo.
    /// </summary>
    /// <seealso cref="Duende.IdentityServer.Stores.IPersistedGrantStore" />
    public class PersistedGrantStore : IPersistedGrantStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly UnitOfWork UnitOfWork;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedGrantStore"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitOfWork.</param>
        /// <param name="logger">The logger.</param>
        public PersistedGrantStore(UnitOfWork unitOfWork, ILogger<PersistedGrantStore> logger)
        {
            UnitOfWork = unitOfWork;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task StoreAsync(PersistedGrant token)
        {
            var existing = await UnitOfWork.Query<XpoPersistedGrant>().SingleOrDefaultAsync(x => x.Key == token.Key);
            if (existing == null)
            {
                Logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

                var persistedGrant = token.ToEntity(UnitOfWork);
                await UnitOfWork.SaveAsync(persistedGrant);
            }
            else
            {
                Logger.LogDebug("{persistedGrantKey} found in database", token.Key);

                token.UpdateEntity(existing);
                await UnitOfWork.SaveAsync(existing);
            }

            try
            {
                await UnitOfWork.CommitChangesAsync();
            }
            catch (LockingException ex)
            {
                Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = await UnitOfWork.Query<XpoPersistedGrant>().SingleOrDefaultAsync(x => x.Key == key);
            var model = persistedGrant?.ToModel();

            Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return model;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var persistedGrants = await Filter(UnitOfWork.Query<XpoPersistedGrant>(), filter).ToArrayAsync();

            //TODO: not sure if we need this
            //persistedGrants = Filter(persistedGrants.AsQueryable(), filter).ToArray();

            var model = persistedGrants.Select(x => x.ToModel());

            Logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Length, filter);

            return model;
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAsync(string key)
        {
            var persistedGrant = await UnitOfWork.Query<XpoPersistedGrant>().SingleOrDefaultAsync(x => x.Key == key);

            if (persistedGrant != null)
            {
                Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

                await UnitOfWork.DeleteAsync(persistedGrant);

                try
                {
                    await UnitOfWork.CommitChangesAsync();
                }
                catch (LockingException ex)
                {
                    Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
                }
            }
            else
            {
                Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var persistedGrants = await Filter(UnitOfWork.Query<XpoPersistedGrant>(), filter).ToArrayAsync();
            //Not sure if we need this
            //persistedGrants = Filter(persistedGrants.AsQueryable(), filter).ToArray();

            Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", persistedGrants.Length, filter);

            await UnitOfWork.DeleteAsync(persistedGrants);

            try
            {
                await UnitOfWork.CommitChangesAsync();
            }
            catch (LockingException ex)
            {
                Logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {@filter}: {error}", persistedGrants.Length, filter, ex.Message);
            }
        }


        private IQueryable<XpoPersistedGrant> Filter(IQueryable<XpoPersistedGrant> query, PersistedGrantFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.ClientId))
            {
                query = query.Where(x => x.ClientId == filter.ClientId);
            }
            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            return query;
        }
    }
}
