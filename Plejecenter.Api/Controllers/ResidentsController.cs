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
using Plejecenter.Shared.DTOs.ResidentAdminPage;

namespace Plejecenter.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
    public async Task<ActionResult<Resident>> Create(ResidentAdminPageDTO.CreateResidentRequest request)
    {
        var newResident = new Resident
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Alias = request.Alias,
            SocialSecurityNumber = request.SocialSecurityNumber,
            Apartment = request.Apartment,
            Status = request.Status,
            // Set defaults for missing fields to avoid null errors
            ShoppingDay = "",
            PaymentMethod = "",
            ShoppingNotes = "",
            PaymentNotes = "",
            Message = "",
            RiskLevel = 0 ,

            Department = await _db.Departments.FirstOrDefaultAsync()
        };

        _db.Residents.Add(newResident);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newResident.Id }, newResident);
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
