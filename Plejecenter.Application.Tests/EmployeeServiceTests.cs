using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Plejecenter.Application.Services.Employees;
using Plejecenter.Shared.DTOs.EmployePage;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Application.Tests;

/// <summary>
/// Application-layer tests (unit tests).
///
/// Formål:
/// - Teste "use case"-laget uden database og uden webserver.
/// - Vi mocker repository'et (IEmployeeRepository), så testen kun handler om service-logik.
/// </summary>
[TestClass]
public class EmployeeServiceTests
{
    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsNull_ServiceReturnsNull()
    {
        // Arrange
        var repo = new Mock<IEmployeeRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((EmployeePageDTO.EmployeeDto?)null);

        var sut = new EmployeeService(repo.Object);

        // Act
        var result = await sut.GetByIdAsync(123);

        // Assert
        Assert.IsNull(result,
            "Hvis repository'et ikke finder en medarbejder, skal service returnere null (så controller kan svare 404).");
    }

    [TestMethod]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        // Arrange
        var expected = new List<EmployeePageDTO.EmployeeDto>
        {
            new(1, "A", "B", "ab", UserRole.Vikar, true)
        };

        var repo = new Mock<IEmployeeRepository>();
        repo.Setup(r => r.GetAllAsync("x"))
            .ReturnsAsync(expected);

        var sut = new EmployeeService(repo.Object);

        // Act
        var result = await sut.GetAllAsync("x");

        // Assert
        CollectionAssert.AreEqual(expected, result,
            "Service-laget er en del af Clean Architecture flowet; her beviser vi at service videresender kaldet korrekt.");
    }
}

