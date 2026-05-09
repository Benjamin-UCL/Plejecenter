using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using System.Globalization;

namespace Plejecenter.WebApp.Providers;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(IJSRuntime js) => _js = js;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");

            if (string.IsNullOrWhiteSpace(token))
                return Anonymous;

            var claims = ParseClaimsFromJwt(token);

            // Hvis token er udløbet, så rydder vi den, så UI ikke "tror" man er logget ind.
            // JWT exp er Unix time seconds.
            var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (exp is not null && long.TryParse(exp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expSeconds))
            {
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                if (DateTimeOffset.UtcNow >= expUtc)
                {
                    await Logout();
                    return Anonymous;
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return Anonymous;
        }
    }

    public async Task Login(string token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task Logout()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var payload = token.Split('.')[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            Convert.FromBase64String(payload))!;

        return json.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
    }
}