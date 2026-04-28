using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using ModelsLibrary;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ResponsibilitiesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResponsibilitiesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet] // Get All
    public async Task<ActionResult<IEnumerable<Responsibility>>> GetResponsibilities()
    {
        return await _db.Responsibilities.ToListAsync();
    }

    [HttpGet("{id}")] // Get By Id
    public async Task<ActionResult<Responsibility>> GetById(int id)
    {
        var responsibility = await _db.Responsibilities.FindAsync(id);

        if (responsibility == null)
        {
            return NotFound();
        }

        return responsibility;
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, Responsibility responsibility)
    {
        if (id != responsibility.Id)
        {
            return BadRequest();
        }

        _db.Entry(responsibility).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResponsibilityExists(id))
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

    [HttpPost] // Creaet
    public async Task<ActionResult<Responsibility>> Creaet(Responsibility responsibility)
    {
        _db.Responsibilities.Add(responsibility);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetById", new { id = responsibility.Id }, responsibility);
    }

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var responsibility = await _db.Responsibilities.FindAsync(id);
        if (responsibility == null)
        {
            return NotFound();
        }

        _db.Responsibilities.Remove(responsibility);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool ResponsibilityExists(int id)
    {
        return _db.Responsibilities.Any(e => e.Id == id);
    }
}
