using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.TokenCleanup
{
    /// <summary>
    /// Interface to model notifications from the TokenCleanup feature.
    /// </summary>
    public interface IOperationalStoreNotification
    {
        /// <summary>
        /// Notification for persisted grants being removed.
        /// </summary>
        /// <param name="persistedGrants"></param>
        /// <returns></returns>
        Task PersistedGrantsRemovedAsync(IEnumerable<XpoPersistedGrant> persistedGrants);

        /// <summary>
        /// Notification for device codes being removed.
        /// </summary>
        /// <param name="deviceCodes"></param>
        /// <returns></returns>
        Task DeviceCodesRemovedAsync(IEnumerable<XpoDeviceFlowCodes> deviceCodes);
    }
}
