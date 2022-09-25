using System;
using System.Threading.Tasks;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace Xenial.Identity.Xpo.Storage
{
    public class XpoClientStore : IClientStore
    {
        public Task<Client> FindClientByIdAsync(string clientId) => throw new NotImplementedException();
    }
}
