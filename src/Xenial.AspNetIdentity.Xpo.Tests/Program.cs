using System;
using System.IO;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Tests.Stores;

using static Xenial.Tasty;

namespace Xenial.AspNetIdentity.Xpo.Tests
{
    internal class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            InMemoryDataStore.Register();
            var connectionString = InMemoryDataStore.GetConnectionStringInMemory(true);
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(connectionString, AutoCreateOption.DatabaseAndSchema);

            SQLiteConnectionProvider.Register();

            var directory = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            var databases = new[]
            {
                ("InMemory", connectionString),
                ("Sqlite", SQLiteConnectionProvider.GetConnectionString(Path.Combine(directory, $"{Guid.NewGuid()}.db")))
            };

            foreach (var (name, cs) in databases)
            {
                UserStoreTests.Tests(name, cs);
                RoleStoreTests.Tests(name, cs);
            }

            return await Run(args);
        }
    }
}
