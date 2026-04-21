using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using ModelsLibrary;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RemindersController : ControllerBase
{
    private readonly AppDbContext _db;

    public RemindersController(AppDbContext context)
    {
        _db = context;
    }

    // GET: api/Reminders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reminder>>> GetAll()
    {
        return await _db.Reminders.ToListAsync();
    }

    // GET: api/Reminders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Reminder>> GetById(int id)
    {
        var reminder = await _db.Reminders.FindAsync(id);

        if (reminder == null)
        {
            return NotFound();
        }
        return reminder;
    }

    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var reminders = await _db.Reminders
            .Where(r => r.Departments.Any(d => d.Id == departmentId))
            .ToListAsync();

        return Ok(reminders);
    }

    // PUT: api/Reminders/5
    [HttpPut("{id}")]
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

    // POST: api/Reminders
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Reminder>> Create(Reminder reminder)
    {
        _db.Reminders.Add(reminder);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetById", new { id = reminder.Id }, reminder);
    }

    // DELETE: api/Reminders/5
    [HttpDelete("{id}")]
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
