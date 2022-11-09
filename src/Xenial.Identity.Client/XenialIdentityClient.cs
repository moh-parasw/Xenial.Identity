using System;
using System.Net.Http;

namespace Xenial.Identity.Client;

public sealed class XenialIdentityClient
{
    private readonly HttpClient httpClient;

    public XenialIdentityClient(HttpClient httpClient)
        => this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

}
