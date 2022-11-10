using System;
using System.Threading.Tasks;
using System.Linq;

using DevExpress.Xpo;
using Xenial.Identity.Xpo.Storage.Mappers;

namespace Xenial.Identity.Xpo.Storage.Stores; public sealed class ClientFactory : IClientFactory
{
    private readonly UnitOfWork unitOfWork;
    public ClientFactory(UnitOfWork unitOfWork)
        => this.unitOfWork = unitOfWork;

    public async Task<Duende.IdentityServer.Models.Client> CreateClient(Duende.IdentityServer.Models.Client client)
    {
        _ = client ?? throw new ArgumentNullException(nameof(client));

        var xpClient = client.ToEntity(unitOfWork);

        await unitOfWork.SaveAsync(xpClient);
        await unitOfWork.CommitChangesAsync();

        return xpClient.ToModel();
    }
}
