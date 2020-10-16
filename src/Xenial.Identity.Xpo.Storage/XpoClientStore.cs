using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Xenial.Identity.Xpo.Storage
{
    public class XpoClientStore : IClientStore
    {
        public Task<Client> FindClientByIdAsync(string clientId) => throw new NotImplementedException();
    }
}
