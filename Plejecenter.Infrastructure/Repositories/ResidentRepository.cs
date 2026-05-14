using Microsoft.EntityFrameworkCore;
using Plejecenter.Application.Services.Residents;
using Plejecenter.Domain;
using Plejecenter.Infrastructure.Data;
using Plejecenter.Shared.DTOs.DisplayPage;
using Plejecenter.Shared.DTOs.ResidentAdminPage;

namespace Plejecenter.Infrastructure.Repositories;

public class ResidentRepository : IResidentRepository
{
    private readonly AppDbContext _db;

    public ResidentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ResidentDisplayDTO>> GetAllForDisplayAsync()
    {
        return await _db.Residents
            .AsNoTracking()
            .Include(r => r.ScheduleMedications)
                .ThenInclude(sm => sm.MedicationDosage)
                    .ThenInclude(md => md.Medication)
            .Include(r => r.ScheduleMedications)
                .ThenInclude(sm => sm.Days)
            .Include(r => r.PatientTimes)
                .ThenInclude(pt => pt.MedicationDosage)
                    .ThenInclude(md => md.Medication)
            .Select(r => new ResidentDisplayDTO
            {
                Id = r.Id,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Alias = r.Alias,
                Status = r.Status,
                Apartment = r.Apartment,
                RiskLevel = r.RiskLevel,
                ScheduleMedications = r.ScheduleMedications.Select(sm => new ScheduleMedicationDisplayDTO
                {
                    Id = sm.Id,
                    DispenseAt = sm.DispenseAt,
                    IsGiven = sm.IsGiven,
                    MedicationName = sm.MedicationDosage.Medication.PrepName,
                    Dosage = sm.MedicationDosage.Dosage,
                    Days = sm.Days
                }).ToList(),
                PatientTimes = r.PatientTimes.Select(pt => new PatientTimeDisplayDTO
                {
                    Id = pt.Id,
                    DispensedAt = pt.DispensedAt,
                    TimeBetweenDosis = pt.TimeBetweenDosis,
                    Note = pt.Note,
                    MedicationName = pt.MedicationDosage.Medication.PrepName,
                    Dosage = pt.MedicationDosage.Dosage
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<List<ResidentAdminPageDTO.ResidentDto>> GetAllAsync(string? search)
    {
        var q = _db.Residents.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(r =>
                r.FirstName.ToLower().Contains(s) ||
                r.LastName.ToLower().Contains(s) ||
                r.Alias.ToLower().Contains(s));
        }

        return await q
            .OrderBy(r => r.FirstName).ThenBy(r => r.LastName)
            .Select(r => new ResidentAdminPageDTO.ResidentDto(
                r.Id, r.FirstName, r.LastName, r.Alias, r.Apartment, r.Status, r.RiskLevel))
            .ToListAsync();
    }

    public async Task<ResidentAdminPageDTO.ResidentDto?> GetByIdAsync(int id)
    {
        var r = await _db.Residents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return r is null
            ? null
            : new ResidentAdminPageDTO.ResidentDto(
                r.Id, r.FirstName, r.LastName, r.Alias, r.Apartment, r.Status, r.RiskLevel);
    }

    public async Task<ResidentAdminPageDTO.ResidentDto> CreateAsync(ResidentAdminPageDTO.CreateResidentRequest req)
    {
        var r = new Resident
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Alias = req.Alias,
            SocialSecurityNumber = req.SocialSecurityNumber,
            Apartment = req.Apartment,
            Status = req.Status
        };

        _db.Residents.Add(r);
        await _db.SaveChangesAsync();

        return new ResidentAdminPageDTO.ResidentDto(
            r.Id, r.FirstName, r.LastName, r.Alias, r.Apartment, r.Status, r.RiskLevel);
    }

    public async Task<bool> UpdateAsync(int id, ResidentAdminPageDTO.UpdateResidentRequest req)
    {
        var r = await _db.Residents.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return false;

        r.FirstName = req.FirstName;
        r.LastName = req.LastName;
        r.Alias = req.Alias;
        r.Apartment = req.Apartment;
        r.Status = req.Status;
        r.ShoppingDay = req.ShoppingDay;
        r.PaymentMethod = req.PaymentMethod;
        r.ShoppingNotes = req.ShoppingNotes;
        r.PaymentNotes = req.PaymentNotes;
        r.Message = req.Message;
        r.RiskLevel = req.RiskLevel;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetActiveAsync(int id, ResidentAdminPageDTO.SetResidentActiveRequest req)
    {
        var r = await _db.Residents.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return false;

        r.Status = req.Status;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _db.Residents.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return false;

        _db.Residents.Remove(r);
        await _db.SaveChangesAsync();
        return true;
    }
}