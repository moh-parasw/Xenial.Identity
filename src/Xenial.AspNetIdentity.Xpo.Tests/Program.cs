using System;
using System.IO;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Xenial.AspNetIdentity.Xpo.Models;

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
                It($"Can load XPO Classes 1 {name}", async () =>
                {
                    var dataLayer = XpoDefault.GetDataLayer(cs, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
                    using var uow = new UnitOfWork(dataLayer);

                    await uow.UpdateSchemaAsync();

                    var userName = "Manuel";
                    var user = new XpoIdentityUser(uow)
                    {
                        UserName = userName
                    };

                    await uow.SaveAsync(user);
                    await uow.CommitChangesAsync();

                    using var uow2 = new UnitOfWork(dataLayer);


                    var result = await uow2.Query<XpoIdentityUser>().FirstAsync(u => u.UserName == userName);

                    return result.UserName == userName;
                });
            }

            return await Run(args);
        }
    }
}
