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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResidentAdminPageDTO.ResidentDto>>> GetAll()
    {
        var residents = await _db.Residents
            .Include(r => r.PatientTimes)
            .Include(r => r.ScheduleMedications)
                .ThenInclude(sm => sm.MedicationDosage)
                    .ThenInclude(md => md.Medication)
            .ToListAsync();

        return residents.Select(r => new ResidentAdminPageDTO.ResidentDto(
            r.Id,
            r.FirstName,
            r.LastName,
            r.Alias,
            r.Apartment,
            r.Status,
            r.RiskLevel,
            r.ShoppingDay,
            r.ShoppingNotes,
            r.PaymentNotes,
            r.Message,
            r.PatientTimes.Select(pt => new ResidentAdminPageDTO.PatientTimeDto
            {
                Id = pt.Id,
                DispensedAt = pt.DispensedAt,
                Note = pt.Note
            }).ToList(),
            r.ScheduleMedications.Select(sm => new ResidentAdminPageDTO.ScheduleMedicationDto
            {
                Id = sm.Id,
                DispenseAt = sm.DispenseAt,
                IsGiven = sm.IsGiven,
                MedicationName = sm.MedicationDosage?.Medication?.PrepName ?? "Ukendt Medicin",
                Dosage = sm.MedicationDosage?.Dosage ?? "Ingen dosis"
            }).ToList()
            
        )).ToList();
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ResidentAdminPageDTO.ResidentDto dto)
    {
        // 1. Check if IDs match
        if (id != dto.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        // 2. Find the existing resident in the database
        var resident = await _db.Residents.FindAsync(id);
        if (resident == null)
        {
            return NotFound();
        }

        // 3. Map values from DTO to the Database Entity
        resident.FirstName = dto.FirstName;
        resident.LastName = dto.LastName;
        resident.Alias = dto.Alias;
        resident.Status = dto.Status;
        resident.RiskLevel = dto.RiskLevel;
        resident.ShoppingDay = dto.ShoppingDay;
        resident.ShoppingNotes = dto.ShoppingNotes;
        resident.PaymentNotes = dto.PaymentNotes;
        resident.Message = dto.Message;

        // 4. Save changes
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

    // For toggling medication status (given/not given)
    // finder pill i databasen, flipper IsGiven, og gemmer ændringen
    [HttpPut("medication/{id}/toggle")]
    public async Task<IActionResult> ToggleMedication(int id)
    {
        var item = await _db.ScheduleMedications.FindAsync(id);
        if (item == null) return NotFound();

        // Flip the status
        item.IsGiven = !item.IsGiven;
        
        await _db.SaveChangesAsync();
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

    [HttpPost("{id}/patienttimes")]
    public async Task<IActionResult> AddPatientTime(int id, PatientTime entry)
    {
        var resident = await _db.Residents
            .Include(r => r.PatientTimes)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resident == null) return NotFound();

        // Ensure the entry is linked to this resident
        resident.PatientTimes.Add(entry);
        
        await _db.SaveChangesAsync();
        return Ok();
    }

    //til Medcin håndtering i detailcard
    [HttpPost("{id}/medication")]
    public async Task<IActionResult> AddScheduledMedication(int id, [FromBody] AddMedScheduleRequest req)
    {
        var resident = await _db.Residents
            .Include(r => r.ScheduleMedications)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (resident == null) return NotFound("Resident ikke fundet.");

        // 1. Validate the Medication exists (Strict Approach)
        var medication = await _db.Medications.FindAsync(req.MedicationId);
        if (medication == null) return BadRequest("Medicin findes ikke i biblioteket.");

        // 2. Search or Create the Dosage (The "Library" Logic)
        var dosage = await _db.MedicationsDosages
            .FirstOrDefaultAsync(d => EF.Property<int>(d, "MedicationId") == req.MedicationId && d.Dosage == req.Dosage);//because Medication is a navigation property, we use EF.Property to filter by the foreign key

        if (dosage == null)
        {
            dosage = new MedicationDosage 
            { 
                Dosage = req.Dosage, 
                Medication = medication 
            };
            _db.MedicationsDosages.Add(dosage);
            // We save here to ensure the dosage gets an ID
            await _db.SaveChangesAsync();
        }

        // 3. Create the Schedule link
        resident.ScheduleMedications.Add(new ScheduleMedication
        {
            DispenseAt = req.Time,
            IsGiven = false,
            MedicationDosage = dosage
        });

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("medication/{medId}")]
    public async Task<IActionResult> DeleteScheduledMedication(int medId)
    {
        var item = await _db.ScheduleMedications.FindAsync(medId);
        if (item == null) return NotFound();

        _db.ScheduleMedications.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
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

    [HttpDelete("patienttimes/{ptId}")]
    public async Task<IActionResult> DeletePatientTime(int ptId)
    {
        var entry = await _db.PatientTimes.FindAsync(ptId);
        
        if (entry == null) return NotFound();

        _db.PatientTimes.Remove(entry);
        await _db.SaveChangesAsync();
        
        return NoContent();
    }
}
