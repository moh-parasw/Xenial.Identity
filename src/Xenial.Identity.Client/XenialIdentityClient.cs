using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Xenial.Identity.Client;

public sealed record XenialIdentityClient
{
    private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
    {

    };

    private readonly HttpClient httpClient;

    public XenialIdentityClient(HttpClient httpClient)
        => this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<IList<XenialUser>> GetUsersAsync()
    {
        var response = await httpClient.GetAsync("api/management/users");
        response.EnsureSuccessStatusCode();
        var responseStr = await response.Content.ReadAsStringAsync();

        var result = JsonConvert.DeserializeObject<IEnumerable<XenialUser>>(responseStr, serializerSettings);
        return result.ToList().AsReadOnly();
    }
}

public sealed record XenialUser(string Id, string UserName);
