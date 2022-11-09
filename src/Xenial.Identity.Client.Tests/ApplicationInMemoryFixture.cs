using DevExpress.Xpo.DB;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Xenial.Identity.Client.Tests;

[CollectionDefinition(nameof(ApplicationInMemoryFixture))]
public class ApplicationInMemoryCollection : ICollectionFixture<ApplicationInMemoryFixture>
{
}

public sealed class ApplicationInMemoryFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> factory = default!;
    public HttpClient HttpClient { get; private set; } = default!;
    public string ConnectionString { get; private set; } = default!;

    public Task InitializeAsync()
    {
        Program.CreateLogger = false;

        ConnectionString = InMemoryDataStore.GetConnectionStringInMemory(true);

        factory = new IdentityWebApplicationFactory<Program>(this);

        HttpClient = factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await using (factory) { }
    }

    public sealed class IdentityWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly ApplicationInMemoryFixture fixture;

        public IdentityWebApplicationFactory(ApplicationInMemoryFixture fixture)
            => this.fixture = fixture;


        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString
                });
            });

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseEnvironment("Development");
    }
}
