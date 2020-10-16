using System;
using System.IO;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Xenial.Identity.Xpo.Storage.Tests.IntegrationTests;
using Xenial.Identity.Xpo.Storage.Tests.Mappers;
using Xenial.Identity.Xpo.Storage.Tests.TokenCleanup;

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

            SQLiteConnectionProvider.Register();

            var directory = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            var databases = new[]
            {
                ("InMemory", connectionString),
                ("Sqlite", SQLiteConnectionProvider.GetConnectionString(Path.Combine(directory, $"{Guid.NewGuid()}.db")))
            };

            foreach (var (name, cs) in databases)
            {
                ClientStoreTests.Tests(name, cs);
                DeviceFlowStoreTests.Tests(name, cs);
                PersistedGrantStoreTests.Tests(name, cs);
                ResourceStoreTests.Tests(name, cs);

                TokenCleanupTests.Tests(name, cs);
            }

            return await Run(args);
        }
    }
}
