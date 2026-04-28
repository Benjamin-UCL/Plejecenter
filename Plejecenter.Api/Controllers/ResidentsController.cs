using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plejecenter.Domain;
using Microsoft.AspNetCore.Authorization;
using Plejecenter.Infrastructure.Data;

namespace Plejecenter.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ResidentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResidentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet] // Get All
    public async Task<ActionResult<IEnumerable<Resident>>> GetAll()
    {
        return await _db.Residents.ToListAsync();
    }

    [HttpGet("{id}")] // Get By Id
    public async Task<ActionResult<Resident>> GetById(int id)
    {
        var resident = await _db.Residents.FindAsync(id);

        if (resident == null)
        {
            return NotFound();
        }

        return resident;
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, Resident resident)
    {
        if (id != resident.Id)
        {
            return BadRequest();
        }

        _db.Entry(resident).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResidentExists(id))
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
    public async Task<ActionResult<Resident>> Create(Resident resident)
    {
        _db.Residents.Add(resident);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetResident", new { id = resident.Id }, resident);
    }

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var resident = await _db.Residents.FindAsync(id);
        if (resident == null)
        {
            return NotFound();
        }

        _db.Residents.Remove(resident);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool ResidentExists(int id)
    {
        return _db.Residents.Any(e => e.Id == id);
    }
}
