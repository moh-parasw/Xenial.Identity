using Shouldly;


namespace Xenial.Identity.Client.Tests;

[Collection(nameof(ApplicationIntegrationFixture))]
[NCrunch.Framework.Serial]
public sealed record SmokeTests(ApplicationIntegrationFixture Fixture)
{
    [DockerRunningFact]
    public async Task SmokeIndex()
    {
        // Act
        var response = await Fixture.HttpClient.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        // Assert
        if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            throw new Exception(content);
        }
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}
