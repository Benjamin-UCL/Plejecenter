using Microsoft.EntityFrameworkCore;
using Plejecenter.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Plejecenter.Application.Services.Employees;
using Plejecenter.Infrastructure.Data;
using Plejecenter.Infrastructure.Repositories;
using Plejecenter.Application.Services.Residents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("Plejecenter.Infrastructure")));

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IResidentRepository, ResidentRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Under normale kørsler (Docker/dev) vil vi migrere og seed'e databasen.
// Men i integration tests bytter vi DbContext ud (typisk til InMemory),
// og der giver Migrate() ikke mening. Derfor springer vi dette over i "Testing".
if (!app.Environment.IsEnvironment("Testing"))
{
var retries = 10;
while (retries > 0)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        if (!db.Departments.Any())
        {
            db.Departments.Add(new Department { Title = "Cardiology" });
            db.SaveChanges();
        }
        if (!db.Users.Any())
        {
            var dept = db.Departments.First();
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Alias = "testalias",
                Password = BCrypt.Net.BCrypt.HashPassword("testpassword"),
                Role = Plejecenter.Shared.Enums.UserRole.Administrator,
                ActiveDeactive = true
            };
            user.Departments.Add(dept);
            db.Users.Add(user);
            db.SaveChanges();
        }
        // Assign every department-less user to the first department.
        // Handles users that existed before the UserDepartments table was added.
        var firstDept = db.Departments.FirstOrDefault();
        if (firstDept != null)
        {
            var unassigned = db.Users
                .Include(u => u.Departments)
                .Where(u => !u.Departments.Any())
                .ToList();
            if (unassigned.Any())
            {
                foreach (var u in unassigned)
                    u.Departments.Add(firstDept);
                db.SaveChanges();
            }
        }
        break;
    }
    catch
    {
        retries--;
        Thread.Sleep(3000);
    }
}
}


app.Run();
