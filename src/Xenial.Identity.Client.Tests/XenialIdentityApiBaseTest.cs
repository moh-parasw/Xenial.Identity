namespace Xenial.Identity.Client.Tests;

public abstract record XenialIdentityApiBaseTest : IAsyncLifetime
{
    protected ApplicationInMemoryFixture Fixture { get; private set; }

    public XenialIdentityClient Client { get; private set; } = default!;

    public XenialIdentityApiBaseTest(ApplicationInMemoryFixture fixture)
        => Fixture = fixture;

    public virtual async Task InitializeAsync()
    {
        await Fixture.InitializeAsync();
        Client = new XenialIdentityClient(Fixture.HttpClient);
    }
    public virtual Task DisposeAsync() => Fixture.DisposeAsync();
}
