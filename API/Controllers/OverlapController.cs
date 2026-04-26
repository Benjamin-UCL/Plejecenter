using API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelsLibrary;
using System.Threading.Tasks;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OverlapController : Controller
{
    private readonly AppDbContext _db;
    public OverlapController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet] // Get all
    public IActionResult GetAll()
    {
        return Ok(_db.Overlaps.ToList());
    }

    [HttpGet("{id}")] // Get by id
    public async Task<IActionResult> GetById(int id)
    {
        var overlap = await _db.Overlaps.FindAsync(id);
        if (overlap == null)
        {
            return NotFound();
        }
        return Ok(overlap);
    }

    [HttpPost] // Create
    public async Task<ActionResult<Overlap>> Create(Overlap overlap)
    {
        _db.Overlaps.Add(overlap);
        await _db.SaveChangesAsync();
        return CreatedAtAction("GetById", new { id = overlap.Id }, overlap);
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, Overlap overlap)
    {
        if(id != overlap.Id) return BadRequest();

        _db.Entry(overlap).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) 
        { 
            if (!OverlapExists(id))
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

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var overlap = await _db.Overlaps.FindAsync(id);
        if(overlap == null)
        {
            return NotFound();
        }

        _db.Overlaps.Remove(overlap);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private bool OverlapExists(int id)
    {
        return _db.Overlaps.Any(overlap => overlap.Id == id);
    }


}
