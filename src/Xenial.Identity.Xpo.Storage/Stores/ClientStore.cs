using System;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Stores
{
    /// <summary>
    /// Implementation of IClientStore thats uses Xpo.
    /// </summary>
    /// <seealso cref="IClientStore" />
    public class ClientStore : IClientStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly UnitOfWork UnitOfWork;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger<ClientStore> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStore"/> class.
        /// </summary>
        /// <param name="unitOfWork">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public ClientStore(UnitOfWork unitOfWork, ILogger<ClientStore> logger)
        {
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            Logger = logger;
        }

        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public virtual async Task<Client> FindClientByIdAsync(string clientId)
        {
            var baseQuery = UnitOfWork.Query<XpoClient>()
                 .Where(x => x.ClientId == clientId);

            var client = await baseQuery.SingleOrDefaultAsync(x => x.ClientId == clientId);

            if (client == null)
            {
                return null;
            }

            var model = client.ToModel();

            Logger.LogDebug("{clientId} found in database: {clientIdFound}", clientId, model != null);

            return model;
        }
    }
}
