using Shouldly;

namespace Xenial.Identity.Client.Tests.UserManagementApi;

[Collection(nameof(ApplicationInMemoryFixture))]
public sealed record UserManagementApiTests(ApplicationInMemoryFixture Fixture) : XenialIdentityApiBaseTest(Fixture)
{
    [Fact]
    public async Task GetUsersReturnsAllWhenAdmin()
    {
        var users = await Client.GetUsersAsync();
        users.ShouldNotBeNull();
        users.Count.ShouldBe(1);
    }
}
