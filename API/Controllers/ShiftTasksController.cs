using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using ModelsLibrary;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftTasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShiftTasksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ShiftTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftTask>>> GetAll()
        {
            return await _context.ShiftTasks.ToListAsync();
        }

        // GET: api/ShiftTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftTask>> GetById(int id)
        {
            var shiftTask = await _context.ShiftTasks.FindAsync(id);

            if (shiftTask == null)
            {
                return NotFound();
            }

            return shiftTask;
        }

        // PUT: api/ShiftTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ShiftTask shiftTask)
        {
            if (id != shiftTask.Id)
            {
                return BadRequest();
            }

            _context.Entry(shiftTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

        // POST: api/ShiftTasks/Create
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ShiftTask>> Create(ShiftTask shiftTask)
        {
            _context.ShiftTasks.Add(shiftTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetById", new { id = shiftTask.Id }, shiftTask);
        }

        // DELETE: api/ShiftTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var shiftTask = await _context.ShiftTasks.FindAsync(id);
            if (shiftTask == null)
            {
                return NotFound();
            }

            _context.ShiftTasks.Remove(shiftTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShiftTaskExists(int id)
        {
            return _context.ShiftTasks.Any(e => e.Id == id);
        }
    }
}
