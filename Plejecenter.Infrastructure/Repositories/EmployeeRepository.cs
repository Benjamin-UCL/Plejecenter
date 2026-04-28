using Microsoft.EntityFrameworkCore;
using Plejecenter.Domain;
using Plejecenter.Application.Services.Employees;
using Plejecenter.Infrastructure.Data;
using Plejecenter.Shared.DTOs.EmployePage;

namespace Plejecenter.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<EmployeePageDTO.EmployeeDto>> GetAllAsync(string? search)
    {
        var q = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                u.Alias.ToLower().Contains(s));
        }

        return await q
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Select(u => new EmployeePageDTO.EmployeeDto(
                u.Id, u.FirstName, u.LastName, u.Alias, u.Role, u.ActiveDeactive))
            .ToListAsync();
    }

    public async Task<EmployeePageDTO.EmployeeDto?> GetByIdAsync(int id)
    {
        var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return u is null
            ? null
            : new EmployeePageDTO.EmployeeDto(u.Id, u.FirstName, u.LastName, u.Alias, u.Role, u.ActiveDeactive);
    }

    public async Task<EmployeePageDTO.EmployeeDto> CreateAsync(EmployeePageDTO.CreateEmployeeRequest req)
    {
        var u = new User
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Alias = req.Alias,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role,
            ActiveDeactive = true
        };

        _db.Users.Add(u);
        await _db.SaveChangesAsync();

        return new EmployeePageDTO.EmployeeDto(u.Id, u.FirstName, u.LastName, u.Alias, u.Role, u.ActiveDeactive);
    }

    public async Task<bool> UpdateAsync(int id, EmployeePageDTO.UpdateEmployeeRequest req)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return false;

        u.FirstName = req.FirstName;
        u.LastName = req.LastName;
        u.Alias = req.Alias;
        u.Role = req.Role;
        if (!string.IsNullOrWhiteSpace(req.Password))
        {
            u.Password = BCrypt.Net.BCrypt.HashPassword(req.Password);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetActiveAsync(int id, EmployeePageDTO.SetEmployeeActiveRequest req)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return false;

        u.ActiveDeactive = req.ActiveDeactive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return false;

        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        return true;
    }
}

