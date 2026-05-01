using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plejecenter.Domain;
using Plejecenter.Infrastructure.Data;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Api.Tests.Infrastructure;

/// <summary>
/// En "mini-host" af vores API, som kører i memory til tests.
///
/// Formål:
/// - Vi kan teste endpoints som rigtige HTTP-kald (integration tests).
/// - Vi kan teste sikkerhed: 401 (ingen token) og 403 (forkert rolle).
/// - Vi kan bruge en InMemory database, så vi ikke er afhængige af Docker/SQL Server i CI.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Plejecenter.Api.Program>
{
    public const string JwtIssuer = "plejecenter-tests";
    public const string JwtAudience = "plejecenter-tests";
    public const string JwtKey = "THIS_IS_A_TEST_KEY_ONLY_CHANGE_ME_1234567890";

    // Kendte test-brugere (aliases/passwords) som vi seed'er i InMemory DB
    public const string AdminAlias = "admin";
    public const string UserAlias = "user";
    public const string Password = "testpassword";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Vi sætter et "Testing" environment, så Program.cs kan springe migrations/seed over.
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(cfg =>
        {
            // Overstyr JWT-konfigurationen, så vi har stabile værdier i tests.
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Key"] = JwtKey,
            });
        });

        builder.ConfigureServices(services =>
        {
            // 1) Fjern den "rigtige" SQL Server DbContext registration.
            var existing = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (existing is not null)
                services.Remove(existing);

            // 2) Tilføj InMemory DbContext til tests.
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("PlejecenterTests"));

            // 3) Seed test-data (brugere + department) så endpoints har noget at arbejde med.
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            if (!db.Departments.Any())
            {
                db.Departments.Add(new Department { Title = "TestDept" });
            }

            if (!db.Users.Any())
            {
                db.Users.AddRange(
                    new User
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Alias = AdminAlias,
                        Password = BCrypt.Net.BCrypt.HashPassword(Password),
                        Role = UserRole.Administrator,
                        ActiveDeactive = true
                    },
                    new User
                    {
                        FirstName = "Normal",
                        LastName = "User",
                        Alias = UserAlias,
                        Password = BCrypt.Net.BCrypt.HashPassword(Password),
                        Role = UserRole.Vikar,
                        ActiveDeactive = true
                    }
                );
            }

            db.SaveChanges();
        });
    }

    /// <summary>
    /// Hjælpemetode: logger ind via /api/auth/login og returnerer en HttpClient,
    /// der allerede har Authorization-header sat.
    ///
    /// Bemærk: Vi bruger bevidst "rigtige" login-endpoint i tests,
    /// så vi tester auth-flowet som det fungerer i praksis.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string alias)
    {
        var client = CreateClient();

        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new { Alias = alias, Password });
        loginResp.EnsureSuccessStatusCode();

        var tokenObj = await loginResp.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokenObj?.Token is null)
            throw new InvalidOperationException("Login gav ikke et token. Tjek AuthController/Login kontrakten.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenObj.Token);
        return client;
    }

    private sealed record TokenResponse(string Token);
}

