using System;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Xenial.Identity.Xpo.Storage.Tests.IntegrationTests;
using Xenial.Identity.Xpo.Storage.Tests.Mappers;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests
{
    internal class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            InMemoryDataStore.Register();
            var connectionString = InMemoryDataStore.GetConnectionStringInMemory(true);
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(connectionString, AutoCreateOption.DatabaseAndSchema);

            ApiResourceMappersTests.Tests();
            PersistedGrantMappersTests.Tests();
            ScopesMappersTests.Tests();
            IdentityResourceMappersTests.Tests();
            ClientMappersTests.Tests();

            ClientStoreTests.Tests("InMemory", connectionString);

            return await Run(args);
        }
    }
}
