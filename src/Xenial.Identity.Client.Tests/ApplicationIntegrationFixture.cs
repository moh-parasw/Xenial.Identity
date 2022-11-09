using DevExpress.Xpo.DB;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Mvc.Testing;

using static DevExpress.Data.Filtering.Helpers.SubExprHelper.ThreadHoppingFiltering;

namespace Xenial.Identity.Client.Tests;

[CollectionDefinition(nameof(ApplicationIntegrationFixture))]
public class ApplicationIntegrationCollection : ICollectionFixture<ApplicationIntegrationFixture>
{
}

public sealed class ApplicationIntegrationFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> factory = default!;
    public HttpClient HttpClient { get; private set; } = default!;
    public MySqlTestcontainer Database { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        Program.CreateLogger = false;

        Database = new TestcontainersBuilder<MySqlTestcontainer>()
            .WithDatabase(new MySqlTestcontainerConfiguration()
            {
                Database = "Xenial.Identity",
                Username = "xenial.identity",
                Password = "!identity32",
            }).Build();

        await Database.StartAsync();

        factory = new IdentityWebApplicationFactory<Program>(this);

        HttpClient = factory.CreateClient();

        var handler = factory.Services.GetRequiredService<DatabaseUpdateHandler>();

        await handler.UpdateDatabase();
    }

    public async Task DisposeAsync()
    {
        await using (factory) { }
        await Database.StopAsync();
        await using (Database) { }
    }

    public sealed class IdentityWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly ApplicationIntegrationFixture fixture;

        public IdentityWebApplicationFactory(ApplicationIntegrationFixture fixture)
            => this.fixture = fixture;


        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"{DataStoreBase.XpoProviderTypeParameterName}={MySqlConnectionProvider.XpoProviderTypeString};{fixture.Database.ConnectionString}"
                });
            });

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseEnvironment("Development");
    }
}
