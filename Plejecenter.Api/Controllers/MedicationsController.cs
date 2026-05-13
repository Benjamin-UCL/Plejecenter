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
using Plejecenter.Shared.DTOs.ResidentAdminPage;

namespace Plejecenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MedicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MedicationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResidentAdminPageDTO.MedicationDto>>> GetMedications()
    {
        return await _db.Medications
            .Select(m => new ResidentAdminPageDTO.MedicationDto 
            { 
                Id = m.Id, 
                PrepName = m.PrepName 
            })
            .ToListAsync();
    }

    [HttpGet("{id}")] // Get by Id
    public async Task<ActionResult<Medication>> GetById(int id)
    {
        var medication = await _db.Medications.FindAsync(id);

        if (medication == null)
        {
            return NotFound();
        }

        return medication;
    }

    [HttpPut("{id}")] // Update
    public async Task<IActionResult> Update(int id, Medication medication)
    {
        if (id != medication.Id)
        {
            return BadRequest();
        }

        _db.Entry(medication).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MedicationExists(id))
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
    public async Task<ActionResult<Medication>> Create(Medication medication)
    {
        _db.Medications.Add(medication);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetById", new { id = medication.Id }, medication);
    }

    [HttpDelete("{id}")] // Delete
    public async Task<IActionResult> Delete(int id)
    {
        var medication = await _db.Medications.FindAsync(id);
        if (medication == null)
        {
            return NotFound();
        }

        _db.Medications.Remove(medication);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool MedicationExists(int id)
    {
        return _db.Medications.Any(e => e.Id == id);
    }
}
