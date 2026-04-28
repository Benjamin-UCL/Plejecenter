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
public class PhoneAssignmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PhoneAssignmentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet] // Get All
    public async Task<ActionResult<IEnumerable<PhoneAssignment>>> GetAll()
    {
        return await _db.PhoneAssignments.ToListAsync();
    }
    
    [HttpGet("{id}")] // Get By Id
    public async Task<ActionResult<PhoneAssignment>> GetById(int id)
    {
        var phoneAssignment = await _db.PhoneAssignments.FindAsync(id);

        if (phoneAssignment == null)
        {
            return NotFound();
        }

        return phoneAssignment;
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, PhoneAssignment phoneAssignment)
    {
        if (id != phoneAssignment.Id)
        {
            return BadRequest();
        }

        _db.Entry(phoneAssignment).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PhoneAssignmentExists(id))
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

    [HttpPost] //Create
    public async Task<ActionResult<PhoneAssignment>> Create(PhoneAssignment phoneAssignment)
    {
        _db.PhoneAssignments.Add(phoneAssignment);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetById", new { id = phoneAssignment.Id }, phoneAssignment);
    }

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var phoneAssignment = await _db.PhoneAssignments.FindAsync(id);
        if (phoneAssignment == null)
        {
            return NotFound();
        }

        _db.PhoneAssignments.Remove(phoneAssignment);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool PhoneAssignmentExists(int id)
    {
        return _db.PhoneAssignments.Any(e => e.Id == id);
    }
}
