using Microsoft.EntityFrameworkCore;
using Plejecenter.Infrastructure.Data;

Console.WriteLine(":::TEST-APPLICATION STARTED:::");
Console.WriteLine();

var connectionString = "Server=localhost,1433;Database=PlejecenterDb;User Id=sa;Password=Dev_Password123;TrustServerCertificate=True;";

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var db = new AppDbContext(options);

var users = db.Users
    .Include(u => u.Departments)
    .Include(u => u.Responsibilities)
        .ThenInclude(r => r.Overlap)
            .ThenInclude(o => o.department)
    .ToList();

if (users.Count == 0)
{
    Console.WriteLine("No users found in the database.");
}

foreach (var user in users)
{
    Console.WriteLine($"┌─ {user.FirstName} {user.LastName}");
    Console.WriteLine($"│  Alias:  {user.Alias}");
    Console.WriteLine($"│  Role:   {user.Role}");
    Console.WriteLine($"│  Active: {user.ActiveDeactive}");

    Console.WriteLine($"│");
    Console.WriteLine($"│  Departments ({user.Departments.Count}):");
    if (user.Departments.Count == 0)
        Console.WriteLine($"│    (none)");
    foreach (var dept in user.Departments)
        Console.WriteLine($"│    • {dept.Title}  (id {dept.Id})");

    Console.WriteLine($"│");
    Console.WriteLine($"│  Responsibilities ({user.Responsibilities.Count}):");
    if (user.Responsibilities.Count == 0)
        Console.WriteLine($"│    (none)");
    foreach (var resp in user.Responsibilities)
    {
        var deptName = resp.Overlap?.department?.Title ?? "—";
        Console.WriteLine($"│    • {resp.Title}  →  overlap {resp.Overlap?.Id}, dept: {deptName}");
    }

    Console.WriteLine($"└─────────────────────────────");
    Console.WriteLine();
}

Console.WriteLine("Done. Press any key to exit.");
Console.ReadKey();
