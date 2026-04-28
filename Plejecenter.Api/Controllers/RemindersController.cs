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
public class RemindersController : ControllerBase
{
    private readonly AppDbContext _db;

    public RemindersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet] // Get All
    public async Task<ActionResult<IEnumerable<Reminder>>> GetAll()
    {
        return await _db.Reminders.ToListAsync();
    }

    [HttpGet("{id}")] // Get by id
    public async Task<ActionResult<Reminder>> GetById(int id)
    {
        var reminder = await _db.Reminders.FindAsync(id);

        if (reminder == null)
        {
            return NotFound();
        }
        return reminder;
    }

    [HttpGet("department/{departmentId}")] // Get by Department
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var reminders = await _db.Reminders
            .Where(r => r.Departments.Any(d => d.Id == departmentId))
            .ToListAsync();

        return Ok(reminders);
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, Reminder reminder)
    {
        if (id != reminder.Id) return BadRequest();            

        _db.Entry(reminder).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReminderExists(id))
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
    public async Task<ActionResult<Reminder>> Create(Reminder reminder)
    {
        _db.Reminders.Add(reminder);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetById", new { id = reminder.Id }, reminder);
    }

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var reminder = await _db.Reminders.FindAsync(id);
        if (reminder == null)
        {
            return NotFound();
        }

        _db.Reminders.Remove(reminder);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool ReminderExists(int id)
    {
        return _db.Reminders.Any(e => e.Id == id);
    }
}
