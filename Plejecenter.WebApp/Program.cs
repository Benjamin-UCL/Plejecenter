using Plejecenter.WebApp.Components;
using Plejecenter.WebApp.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Plejecenter.WebApp.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.JSInterop;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthStateProvider>());

// Named client used by EmployePage and ResidentAdmin via IHttpClientFactory.CreateClient("SlottetApi").
// Those pages attach the Bearer token manually in OnAfterRenderAsync, so no handler is needed here.
// Calling AddHttpClient also registers IHttpClientFactory in the container.
builder.Services.AddHttpClient("SlottetApi", client =>
{
    client.BaseAddress = new Uri("http://api:8080/");
});

// Default HttpClient (@inject HttpClient) is created directly from the circuit scope so that
// AuthHeaderHandler gets the real circuit-aware IJSRuntime. IHttpClientFactory creates handlers
// in its own internal scope, giving an UnsupportedJavaScriptRuntime that silently swallows every
// localStorage call and never attaches the Bearer token.
builder.Services.AddScoped(sp =>
{
    var handler = new AuthHeaderHandler(
        sp.GetRequiredService<IJSRuntime>(),
        sp.GetRequiredService<JwtAuthStateProvider>())
    {
        InnerHandler = new HttpClientHandler()
    };
    return new HttpClient(handler) { BaseAddress = new Uri("http://api:8080/") };
});

builder.Services.AddScoped<AppState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Avoid stale scoped CSS/JS during frequent rebuilds (esp. in Docker).
        // Without cache-busting, browsers may keep an old *.styles.css that no longer matches scope attributes.
        var path = ctx.Context.Request.Path.Value ?? "";
        if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.CacheControl = "no-store, max-age=0";
        }
    }
});
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();
app.Run();