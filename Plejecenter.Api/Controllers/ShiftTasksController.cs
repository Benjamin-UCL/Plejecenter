using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plejecenter.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plejecenter.Infrastructure.Data;

namespace Plejecenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShiftTasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public ShiftTasksController(AppDbContext db)
    {
        _db = db;
    }

    
    [HttpGet] // Get All
    public async Task<ActionResult<IEnumerable<ShiftTask>>> GetAll()
    {
        return await _db.ShiftTasks.ToListAsync();
    }

    [HttpGet("{id}")] // Get by Id
    public async Task<ActionResult<ShiftTask>> GetById(int id)
    {
        var shiftTask = await _db.ShiftTasks.FindAsync(id);

        if (shiftTask == null)
        {
            return NotFound();
        }

        return shiftTask;
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, ShiftTask shiftTask)
    {
        if (id != shiftTask.Id)
        {
            return BadRequest();
        }

        _db.Entry(shiftTask).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ShiftTaskExists(id))
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
    public async Task<ActionResult<ShiftTask>> Create(ShiftTask shiftTask)
    {
        _db.ShiftTasks.Add(shiftTask);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetById", new { id = shiftTask.Id }, shiftTask);
    }

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var shiftTask = await _db.ShiftTasks.FindAsync(id);
        if (shiftTask == null)
        {
            return NotFound();
        }

        _db.ShiftTasks.Remove(shiftTask);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool ShiftTaskExists(int id)
    {
        return _db.ShiftTasks.Any(e => e.Id == id);
    }
}
