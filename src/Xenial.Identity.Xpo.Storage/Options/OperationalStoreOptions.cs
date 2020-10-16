using System;
using System.Collections.Generic;
using System.Text;

namespace Xenial.Identity.Xpo.Storage.Options
{
    /// <summary>
    /// Options for configuring the operational context.
    /// </summary>
    public class OperationalStoreOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether stale entries will be automatically cleaned up from the database.
        /// This is implemented by periodically connecting to the database (according to the TokenCleanupInterval) from the hosting application.
        /// Defaults to false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable token cleanup]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableTokenCleanup { get; set; } = false;

        /// <summary>
        /// Gets or sets the token cleanup interval (in seconds). The default is 3600 (1 hour).
        /// </summary>
        /// <value>
        /// The token cleanup interval.
        /// </value>
        public int TokenCleanupInterval { get; set; } = 3600;

        /// <summary>
        /// Gets or sets the number of records to remove at a time. Defaults to 100.
        /// </summary>
        /// <value>
        /// The size of the token cleanup batch.
        /// </value>
        public int TokenCleanupBatchSize { get; set; } = 100;
    }
}
