using System.Threading.Tasks;

namespace Xenial.Identity.Xpo.Storage.Stores;

public interface IClientFactory
{
    Task<Duende.IdentityServer.Models.Client> CreateClient(Duende.IdentityServer.Models.Client client);
}
