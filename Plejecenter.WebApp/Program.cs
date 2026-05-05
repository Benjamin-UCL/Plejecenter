using Plejecenter.WebApp.Components;
using Plejecenter.WebApp.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Plejecenter.WebApp.Services;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddScoped<AuthHeaderHandler>();

// Named HttpClient for pages/components that use IHttpClientFactory
builder.Services.AddHttpClient("SlottetApi", client =>
{
    client.BaseAddress = new Uri("http://api:8080/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

// Default HttpClient (used by Login.razor via @inject HttpClient Http)
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SlottetApi"));

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