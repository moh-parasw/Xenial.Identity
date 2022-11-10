using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

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

    public Task<XenialResult<IEnumerable<XenialUser>>> GetUsersAsync()
         => GetAsync<IEnumerable<XenialUser>>("api/management/users");

    private async Task<XenialResult<TData>> GetAsync<TData>(string route)
    {
        try
        {
            var response = await httpClient.GetAsync(route);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var problemsStr = await response.Content.ReadAsStringAsync();
                var problems = JsonConvert.DeserializeObject<ProblemDetails>(problemsStr, serializerSettings);

                return new XenialResult<TData>.Error(new XenialBadRequestException(problems!));
            }

            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TData>(responseStr, serializerSettings);
            return result!;
        }
        catch (Exception ex)
        {
            return new XenialResult<TData>.Error(new XenialUnknownApiException(ex));
        }
    }
}

public sealed record XenialUser(
    string Id,
    string UserName
);

public sealed record CreateXenialUserRequest(
    string Email,
    string? Password = null
);


public abstract class XenialApiException : Exception
{
    public XenialApiException()
    {

    }

    public XenialApiException(string message) : base(message)
    {

    }

    public XenialApiException(Exception innerException) : base(null, innerException)
    {

    }
}

public sealed class XenialUnknownApiException : XenialApiException
{
    public XenialUnknownApiException(Exception innerException) : base(innerException)
    {

    }
}

public sealed class XenialBadRequestException : XenialApiException
{
    public ProblemDetails Details { get; private set; }
    public XenialBadRequestException(ProblemDetails details)
        => Details = details;
}
