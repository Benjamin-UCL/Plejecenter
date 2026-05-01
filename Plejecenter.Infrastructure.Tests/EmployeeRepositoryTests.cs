using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plejecenter.Domain;
using Plejecenter.Infrastructure.Data;
using Plejecenter.Infrastructure.Repositories;
using Plejecenter.Shared.DTOs.EmployePage;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Infrastructure.Tests;

/// <summary>
/// Infrastructure-layer tests (repository + EF Core).
///
/// Formål:
/// - Teste at repository'et faktisk kan gemme/hente data via EF Core.
/// - Vi bruger InMemory EF provider, så testen kører hurtigt og uden SQL Server.
/// </summary>
[TestClass]
public class EmployeeRepositoryTests
{
    [TestMethod]
    public async Task CreateAsync_PersistsUser_AndReturnsDto()
    {
        // Arrange: InMemory DbContext
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "EmployeeRepositoryTests_Create")
            .Options;

        await using var db = new AppDbContext(options);
        var repo = new EmployeeRepository(db);

        var req = new EmployeePageDTO.CreateEmployeeRequest(
            FirstName: "Test",
            LastName: "User",
            Alias: "testuser",
            Password: "123456",
            Role: UserRole.Vikar
        );

        // Act
        var dto = await repo.CreateAsync(req);

        // Assert: DTO er returneret
        Assert.AreEqual("Test", dto.FirstName);
        Assert.AreEqual("testuser", dto.Alias);

        // Assert: entity ligger i databasen og password er hashed (ikke raw)
        var entity = await db.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        Assert.IsNotNull(entity, "Efter CreateAsync skal user ligge i databasen.");
        Assert.AreNotEqual("123456", entity!.Password, "Password bør ikke gemmes i klartekst.");
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify("123456", entity.Password), "Password hash skal matche den originale kode.");
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "EmployeeRepositoryTests_GetById_NotFound")
            .Options;

        await using var db = new AppDbContext(options);
        var repo = new EmployeeRepository(db);

        var dto = await repo.GetByIdAsync(999);
        Assert.IsNull(dto, "Hvis en medarbejder ikke findes, skal repository'et returnere null.");
    }
}

