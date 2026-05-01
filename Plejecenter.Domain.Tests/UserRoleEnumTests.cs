using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Domain.Tests;

/// <summary>
/// Domain-layer tests (små og hurtige).
///
/// Formål:
/// - Vise at vi også kan teste "rene" domæne-ting uden database/web.
/// - Her tjekker vi at enum-værdierne er stabile, fordi de ofte gemmes som int i DB.
/// </summary>
[TestClass]
public class UserRoleEnumTests
{
    [TestMethod]
    public void Administrator_ShouldBeZero_ToKeepDbValuesStable()
    {
        // Hvis enum values ændres (eller re-ordered), kan eksisterende DB-data pege på forkerte roller.
        Assert.AreEqual(0, (int)UserRole.Administrator);
    }
}

