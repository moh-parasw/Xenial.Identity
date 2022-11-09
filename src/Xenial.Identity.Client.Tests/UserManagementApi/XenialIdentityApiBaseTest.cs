namespace Xenial.Identity.Client.Tests.UserManagementApi;

public abstract record XenialIdentityApiBaseTest
{
    protected ApplicationInMemoryFixture Fixture { get; private set; }
    public XenialIdentityClient Client { get; private set; }
    public XenialIdentityApiBaseTest(ApplicationInMemoryFixture fixture)
    {
        Fixture = fixture;
        Client = new XenialIdentityClient(fixture.HttpClient);
    }
}
