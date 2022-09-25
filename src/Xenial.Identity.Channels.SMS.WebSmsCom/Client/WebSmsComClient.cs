using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Serialization;

namespace Xenial.Identity.Channels.Client;

public class WebSmsComClient
{
    private readonly HttpClient httpClient;
    public WebSmsComClient(HttpClient httpClient)
        => this.httpClient = httpClient;

    public async Task<WebSmsComResponse> SendSms(
        WebSmsComSettings settings,
        WebSmsComTextRequest request,
        CancellationToken cancellationToken = default)
    {
        static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        httpClient.BaseAddress = new Uri(settings.Server);
        if (settings.UseBasicAuth)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64Encode($"{settings.Username}:{settings.Password}"));
        }
        else
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        }

        var result = await httpClient.PostAsJsonAsync("smsmessaging/text", request, cancellationToken);
        result.EnsureSuccessStatusCode();
        return (await result.Content.ReadFromJsonAsync<WebSmsComResponse>(cancellationToken: cancellationToken))!;
    }
}
