using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Xenial.Identity.Xpo.Storage.Tests.IntegrationTests;
using Xenial.Identity.Xpo.Storage.Tests.Mappers;
using Xenial.Identity.Xpo.Storage.Tests.Services;
using Xenial.Identity.Xpo.Storage.Tests.TokenCleanup;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests
{
    internal class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            InMemoryDataStore.Register();
            MySqlConnectionProvider.Register();
            PostgreSqlConnectionProvider.Register();
            SQLiteConnectionProvider.Register();

            var connectionString = InMemoryDataStore.GetConnectionStringInMemory(true);
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(connectionString, AutoCreateOption.DatabaseAndSchema);

            ApiResourceMappersTests.Tests();
            PersistedGrantMappersTests.Tests();
            ScopesMappersTests.Tests();
            IdentityResourceMappersTests.Tests();
            ClientMappersTests.Tests();

            await using var databases = await GetDataBasesAsync(connectionString);

            foreach (var (name, cs) in databases)
            {
                ClientStoreTests.Tests(name, cs);
                DeviceFlowStoreTests.Tests(name, cs);
                PersistedGrantStoreTests.Tests(name, cs);
                ResourceStoreTests.Tests(name, cs);
                TokenCleanupTests.Tests(name, cs);
                CorsPolicyServiceTests.Tests(name, cs);
            }

            return await Run(args);
        }

        private static async Task<DatabaseCollection> GetDataBasesAsync(string inMemoryConnectionString)
        {
            var directory = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            var databases = new DatabaseCollection
            {
                new("InMemory", inMemoryConnectionString),
                new("Sqlite", SQLiteConnectionProvider.GetConnectionString(Path.Combine(directory, $"{Guid.NewGuid()}.db"))),
            };

            if (IsDockerAvailable)
            {

                var postgreDatabase = new TestcontainersBuilder<PostgreSqlTestcontainer>()
                        .WithDatabase(new PostgreSqlTestcontainerConfiguration()
                        {
                            Database = "Xenial.Identity",
                            Username = "xenial.identity",
                            Password = "!identity32",
                        }).Build();

                var mysqlDatabase = new TestcontainersBuilder<MySqlTestcontainer>()
                        .WithDatabase(new MySqlTestcontainerConfiguration()
                        {
                            Database = "Xenial.Identity",
                            Username = "xenial.identity",
                            Password = "!identity32",
                        }).Build();

                Console.WriteLine("Starting Databases...");
                await Task.WhenAll(postgreDatabase.StartAsync(), mysqlDatabase.StartAsync());
                Console.WriteLine("Started Databases.");

                databases.Add(new("Postgre", $"{DataStoreBase.XpoProviderTypeParameterName}={PostgreSqlConnectionProvider.XpoProviderTypeString};{postgreDatabase.ConnectionString}") { Disposable = postgreDatabase });
                databases.Add(new("MySql", $"{DataStoreBase.XpoProviderTypeParameterName}={MySqlConnectionProvider.XpoProviderTypeString};{mysqlDatabase.ConnectionString}") { Disposable = mysqlDatabase });
            }

            return databases;
        }

        private static bool IsDockerAvailable
        {
            get
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NCRUNCH")))
                {
                    return false;
                }

                if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
                {
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
                    {
                        var isDockerRunning = false;
                        SimpleExec.Command.Run("docker", "info", handleExitCode: exitCode =>
                        {
                            isDockerRunning = exitCode == 0;
                            return true;
                        });
                        if (!isDockerRunning)
                        {
                            //Docker is not running. Start Docker to run
                            return false;
                        }
                    }
                    else
                    {
                        //Docker is not supported on Github Actions for windows or macos.
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public sealed class DatabaseCollection : IAsyncDisposable, IEnumerable<TestDatabase>
    {
        private readonly List<TestDatabase> dataBases = new List<TestDatabase>();

        public async ValueTask DisposeAsync()
        {
            foreach (var database in this.Where(m => m.Disposable is not null))
            {
                await database.Disposable.DisposeAsync();
            }
        }

        public void Add(TestDatabase database) => dataBases.Add(database);

        public IEnumerator<TestDatabase> GetEnumerator() => ((IEnumerable<TestDatabase>)dataBases).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dataBases).GetEnumerator();
    }

    public sealed record TestDatabase(string Name, string ConnectionString)
    {
        public IAsyncDisposable Disposable { get; init; }
    }
}
