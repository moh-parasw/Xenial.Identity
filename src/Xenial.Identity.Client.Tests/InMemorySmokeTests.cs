
namespace Xenial.Identity.Client.Tests;

public sealed record InMemorySmokeTests()
     : XenialIdentityApiBaseTest(new ApplicationInMemoryFixture() with { AllowAutoRedirect = true })
{
    [Fact]
    public async Task SmokeIndex()
    {
        // Act
        var response = await Fixture.HttpClient.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}
