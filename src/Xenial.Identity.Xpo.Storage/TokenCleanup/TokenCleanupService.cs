using System;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Models;
using Xenial.Identity.Xpo.Storage.Options;

namespace Xenial.Identity.Xpo.Storage.TokenCleanup
{
    /// <summary>
    /// Helper to cleanup stale persisted grants and device codes.
    /// </summary>
    public class TokenCleanupService
    {
        private readonly OperationalStoreOptions options;
        private readonly UnitOfWork unitOfWork;
        private readonly IOperationalStoreNotification operationalStoreNotification;
        private readonly ILogger<TokenCleanupService> logger;

        /// <summary>
        /// Constructor for TokenCleanupService.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="operationalStoreNotification"></param>
        /// <param name="logger"></param>
        public TokenCleanupService(
            OperationalStoreOptions options,
            UnitOfWork unitOfWork,
            ILogger<TokenCleanupService> logger,
            IOperationalStoreNotification operationalStoreNotification = null)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            if (this.options.TokenCleanupBatchSize < 1)
            {
                throw new ArgumentException("Token cleanup batch size interval must be at least 1");
            }

            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.operationalStoreNotification = operationalStoreNotification;
        }

        /// <summary>
        /// Method to clear expired persisted grants.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveExpiredGrantsAsync()
        {
            try
            {
                logger.LogTrace("Querying for expired grants to remove");

                await RemoveGrantsAsync();
                await RemoveDeviceCodesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Exception removing expired grants: {exception}", ex.Message);
            }
        }

        /// <summary>
        /// Removes the stale persisted grants.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task RemoveGrantsAsync()
        {
            var found = Int32.MaxValue;

            while (found >= options.TokenCleanupBatchSize)
            {
                var expiredGrants = await unitOfWork.Query<XpoPersistedGrant>()
                    .Where(x => x.Expiration < DateTime.UtcNow)
                    .OrderBy(x => x.Key)
                    .Take(options.TokenCleanupBatchSize)
                    .ToArrayAsync();

                found = expiredGrants.Length;
                logger.LogInformation("Removing {grantCount} grants", found);

                if (found > 0)
                {
                    await unitOfWork.DeleteAsync(expiredGrants);
                    await SaveChangesAsync();

                    if (operationalStoreNotification != null)
                    {
                        await operationalStoreNotification.PersistedGrantsRemovedAsync(expiredGrants);
                    }
                }
            }
        }


        /// <summary>
        /// Removes the stale device codes.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task RemoveDeviceCodesAsync()
        {
            var found = Int32.MaxValue;

            while (found >= options.TokenCleanupBatchSize)
            {
                var expiredCodes = await unitOfWork.Query<XpoDeviceFlowCodes>()
                    .Where(x => x.Expiration < DateTime.UtcNow)
                    .OrderBy(x => x.DeviceCode)
                    .Take(options.TokenCleanupBatchSize)
                    .ToArrayAsync();

                found = expiredCodes.Length;
                logger.LogInformation("Removing {deviceCodeCount} device flow codes", found);

                if (found > 0)
                {
                    await unitOfWork.DeleteAsync(expiredCodes);
                    await SaveChangesAsync();

                    if (operationalStoreNotification != null)
                    {
                        await operationalStoreNotification.DeviceCodesRemovedAsync(expiredCodes);
                    }
                }
            }
        }

        private async Task SaveChangesAsync()
        {
            var count = 3;

            while (count > 0)
            {
                try
                {
                    await unitOfWork.CommitChangesAsync();
                    return;
                }
                catch (LockingException ex)
                {
                    count--;

                    // we get this if/when someone else already deleted the records
                    // we want to essentially ignore this, and keep working
                    logger.LogDebug("Concurrency exception removing expired grants: {exception}", ex.Message);

                    //TODO: mark this entry as not attached anymore so we don't try to re-delete
                    //foreach (var entry in ex.Entries)
                    //{
                    //    // mark this entry as not attached anymore so we don't try to re-delete
                    //    entry.State = EntityState.Detached;
                    //}
                }
            }

            logger.LogDebug("Too many concurrency exceptions. Exiting.");
        }
    }
}
