using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Plejecenter.WebApp.Providers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;

    public AuthHeaderHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = null;
        try
        {
            token = await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");
        }
        catch (InvalidOperationException)
        {
            // Happens during server-side prerendering: JS interop isn't available yet.
            // We simply send the request without an auth header.
        }

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}