using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plejecenter.Api.Tests.Infrastructure;
using Plejecenter.Shared.DTOs.EmployePage;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Api.Tests;

/// <summary>
/// Security-focused tests for EmployeePage endpoints (EmployeesController).
///
/// Vi tester specifikt:
/// 1) Authentication: "Er man logget ind?" -> 401 hvis ikke.
/// 2) Authorization: "Har man den rigtige rolle?" -> 403 hvis ikke.
///
/// Format: MSTest (som ønsket).
/// </summary>
[TestClass]
public class EmployeesControllerSecurityTests
{
    private readonly TestWebApplicationFactory _factory = new();

    [TestMethod]
    public async Task GetEmployees_WithoutToken_Returns401_Unauthorized()
    {
        // Arrange: En helt almindelig client uden Authorization header.
        // Formål: bevise at [Authorize] på controlleren faktisk beskytter endpointet.
        var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/api/employees");

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
            "Når man ikke sender en JWT, skal API'et svare 401 (Unauthorized).");
    }

    [TestMethod]
    public async Task CreateEmployee_WithNonAdminRole_Returns403_Forbidden()
    {
        // Arrange: Log ind som en normal bruger (ikke Administrator).
        // Formål: bevise at vores write-endpoint kræver admin-rollen.
        var client = await _factory.CreateAuthenticatedClientAsync(TestWebApplicationFactory.UserAlias);

        var req = new EmployeePageDTO.CreateEmployeeRequest(
            FirstName: "Test",
            LastName: "Employee",
            Alias: "emp1",
            Password: "123456",
            Role: UserRole.Vikar
        );

        // Act
        var resp = await client.PostAsJsonAsync("/api/employees", req);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, resp.StatusCode,
            "En bruger uden Administrator-rolle skal få 403 (Forbidden) på POST /api/employees.");
    }

    [TestMethod]
    public async Task CreateEmployee_WithAdminRole_Returns201_Created()
    {
        // Arrange: Log ind som admin.
        // Formål: bevise at en Administrator må oprette medarbejdere.
        var client = await _factory.CreateAuthenticatedClientAsync(TestWebApplicationFactory.AdminAlias);

        var req = new EmployeePageDTO.CreateEmployeeRequest(
            FirstName: "Test",
            LastName: "Employee",
            Alias: "emp2",
            Password: "123456",
            Role: UserRole.Vikar
        );

        // Act
        var resp = await client.PostAsJsonAsync("/api/employees", req);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode,
            "Admin skal kunne oprette medarbejder og få 201 (Created).");
    }
}

