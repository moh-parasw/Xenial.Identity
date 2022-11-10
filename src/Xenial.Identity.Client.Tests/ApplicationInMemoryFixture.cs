using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

using DevExpress.Xpo.DB;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;

namespace Xenial.Identity.Client.Tests;

[CollectionDefinition(nameof(ApplicationInMemoryFixture))]
public class ApplicationInMemoryCollection : ICollectionFixture<ApplicationInMemoryFixture>
{
}

public sealed record ApplicationInMemoryFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> factory = default!;
    public HttpClient HttpClient { get; private set; } = default!;
    public string ConnectionString { get; private set; } = default!;

    public IServiceProvider Services => factory.Services;

    public bool CreateLogger { get; set; }
    public bool AllowAutoRedirect { get; set; }

    public async Task InitializeAsync()
    {
        Program.CreateLogger = CreateLogger;

        ConnectionString = InMemoryDataStore.GetConnectionStringInMemory(true);

        factory = new IdentityWebApplicationFactory<Program>(this);

        HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = AllowAutoRedirect,
        });

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "TestScheme");

        var handler = factory.Services.GetRequiredService<DatabaseUpdateHandler>();

        await handler.UpdateDatabase();
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
                    ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString,
                    ["Xenial:SeedAdminUser"] = "true",
                });
            });

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseEnvironment("Development");
    }
}
