using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Options;
using Xenial.Identity.Xpo.Storage.TokenCleanup;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Helper to cleanup expired persisted grants.
    /// </summary>
    public class TokenCleanupHost : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly OperationalStoreOptions options;
        private readonly ILogger<TokenCleanupHost> logger;

        private TimeSpan CleanupInterval => TimeSpan.FromSeconds(options.TokenCleanupInterval);

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Constructor for TokenCleanupHost.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public TokenCleanupHost(IServiceProvider serviceProvider, OperationalStoreOptions options, ILogger<TokenCleanupHost> logger)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger;
        }

        /// <summary>
        /// Starts the token cleanup polling.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (options.EnableTokenCleanup)
            {
                if (cancellationTokenSource != null)
                {
                    throw new InvalidOperationException("Already started. Call Stop first.");
                }

                logger.LogDebug("Starting grant removal");

                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                Task.Factory.StartNew(() => StartInternalAsync(cancellationTokenSource.Token));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the token cleanup polling.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (options.EnableTokenCleanup)
            {
                if (cancellationTokenSource == null)
                {
                    throw new InvalidOperationException("Not started. Call Start first.");
                }

                logger.LogDebug("Stopping grant removal");

                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            return Task.CompletedTask;
        }

        private async Task StartInternalAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogDebug("CancellationRequested. Exiting.");
                    break;
                }

                try
                {
                    await Task.Delay(CleanupInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    logger.LogDebug("TaskCanceledException. Exiting.");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError("Task.Delay exception: {0}. Exiting.", ex.Message);
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogDebug("CancellationRequested. Exiting.");
                    break;
                }

                await RemoveExpiredGrantsAsync();
            }
        }

        private async Task RemoveExpiredGrantsAsync()
        {
            try
            {
                using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var tokenCleanupService = serviceScope.ServiceProvider.GetRequiredService<TokenCleanupService>();
                    await tokenCleanupService.RemoveExpiredGrantsAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Exception removing expired grants: {exception}", ex.Message);
            }
        }
    }
}
