using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plejecenter.Domain;
using Plejecenter.Infrastructure.Data;

namespace Plejecenter.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly AppDbContext _db;
    public DepartmentController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet] // Get all
    public async Task<ActionResult<IEnumerable<Department>>> GetAll()
    {
        return await _db.Departments.ToListAsync();
    }

    [HttpGet("{id}")] // Get by id
    public async Task<ActionResult<Department>> GetById(int id)
    {
        var department = await _db.Departments.FindAsync(id);
       
        if (department == null) 
            return NotFound();

        return Ok(department);
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, Department department)
    {
        if (id != department.Id)
            return BadRequest();

        _db.Entry(department).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if(!DepartmentExcists(id))
            { 
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        
        return NoContent();
    }

    [HttpPost] // Create
    public async Task<ActionResult<Department>> Create(Department department)
    {
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetById", new { id = department.Id }, department);
    }

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool DepartmentExcists(int id)
    {
        return _db.Departments.Any(x => x.Id == id);
    }

}
