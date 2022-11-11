using System.Text;

using Microsoft.AspNetCore.Http;
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

    public Task<XenialResult<IEnumerable<XenialUser>>> GetUsersAsync(CancellationToken cancellationToken = default)
        => GetAsync<IEnumerable<XenialUser>>("api/management/users", cancellationToken);

    public Task<XenialResult<XenialUser>> CreateUserAsync(CreateXenialUserRequest request, CancellationToken cancellationToken = default)
        => PostAsync<XenialUser>("api/management/users/create", request, cancellationToken);

    public Task<XenialResult<XenialIdResponse>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        => DeleteAsync<XenialIdResponse>($"api/management/users/{userId}", cancellationToken);

    private async Task<XenialResult<TData>> PostAsync<TData>(string route, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var payloadString = JsonConvert.SerializeObject(payload, typeof(object), serializerSettings);
            var request = new StringContent(payloadString, Encoding.UTF8, "application/json");
            request.Headers.Add("X-XenialClientVersion", ThisAssembly.Info.Version);
            var response = await httpClient.PostAsync(route, request, cancellationToken);

            return await ProcessResponse<TData>(response);
        }
        catch (Exception ex)
        {
            return new XenialResult<TData>.Error(new XenialUnknownApiException(ex));
        }
    }

    private async Task<XenialResult<TData>> GetAsync<TData>(string route, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, route);
            request.Headers.Add("X-XenialClientVersion", ThisAssembly.Info.Version);
            var response = await httpClient.SendAsync(request, cancellationToken);

            return await ProcessResponse<TData>(response);
        }
        catch (Exception ex)
        {
            return new XenialResult<TData>.Error(new XenialUnknownApiException(ex));
        }
    }

    private async Task<XenialResult<TData>> DeleteAsync<TData>(string route, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, route);
            request.Headers.Add("X-XenialClientVersion", ThisAssembly.Info.Version);
            var response = await httpClient.SendAsync(request, cancellationToken);

            return await ProcessResponse<TData>(response);
        }
        catch (Exception ex)
        {
            return new XenialResult<TData>.Error(new XenialUnknownApiException(ex));
        }
    }

    private async Task<XenialResult<TData>> ProcessResponse<TData>(HttpResponseMessage response)
    {
        if (StatusCodes.Status404NotFound == (int)response.StatusCode)
        {
            var problemsStr = await response.Content.ReadAsStringAsync();

            var problems = JsonConvert.DeserializeObject<ProblemDetails>(problemsStr, serializerSettings);

            return new XenialResult<TData>.Error(new XenialNotFoundException(problems!));
        }

        if (StatusCodes.Status422UnprocessableEntity == (int)response.StatusCode)
        {
            var problemsStr = await response.Content.ReadAsStringAsync();

            var problems = JsonConvert.DeserializeObject<ValidationProblemDetails>(problemsStr, serializerSettings);

            return new XenialResult<TData>.Error(new XenialValidationException(problems!));
        }

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

}

