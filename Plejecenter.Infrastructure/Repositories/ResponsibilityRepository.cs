using Microsoft.EntityFrameworkCore;
using Plejecenter.Application.Services.Responsibilities;
using Plejecenter.Domain;
using Plejecenter.Infrastructure.Data;
using Plejecenter.Shared;
using Plejecenter.Shared.DTOs.ResponsibilityPage;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Infrastructure.Repositories;

public class ResponsibilityRepository : IResponsibilityRepository
{
    private readonly AppDbContext _db;

    public ResponsibilityRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ResponsibilityDTO.ResponsibilityDto>> GetAllAsync(DateTime date, ShiftType shift)
    {
        var day = date.Date;

        var templates = await _db.ResponsibilityTemplates
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Id)
            .ToListAsync();

        var applicableTemplates = templates
            .Where(t => ResponsibilitySchedule.TemplateAppliesTo(
                day, shift, t.Recurrence, t.ApplicableShifts, t.StartDate))
            .ToList();

        var existing = await _db.Responsibilities
            .Where(r => r.TaskDate == day && r.Shift == shift)
            .ToListAsync();

        var existingTemplateIds = existing.Select(e => e.TemplateId).ToHashSet();
        if (applicableTemplates.Count > 0)
        {
            var nextSort = existing.Any() ? existing.Max(x => x.SortOrder) : 0;
            var toAdd = new List<Responsibility>();

            foreach (var t in applicableTemplates)
            {
                if (existingTemplateIds.Contains(t.Id)) continue;
                nextSort++;

                toAdd.Add(new Responsibility
                {
                    TemplateId = t.Id,
                    Title = t.Title,
                    TaskDate = day,
                    Shift = shift,
                    SortOrder = nextSort,
                    UserId = null,
                    IsCompleted = false
                });
            }

            if (toAdd.Count > 0)
            {
                _db.Responsibilities.AddRange(toAdd);
                await _db.SaveChangesAsync();
                existing.AddRange(toAdd);
            }
        }

        var templateById = applicableTemplates.ToDictionary(t => t.Id);

        return existing
            .Where(r => templateById.ContainsKey(r.TemplateId))
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id)
            .Select(r =>
            {
                var t = templateById[r.TemplateId];
                return ToDto(r, t.Recurrence, t.ApplicableShifts);
            })
            .ToList();
    }

    public async Task<ResponsibilityDTO.ResponsibilityDto> CreateTemplateAsync(ResponsibilityDTO.CreateTemplateRequest req)
    {
        var day = req.StartDate.Date;
        var applicableShifts = ResponsibilitySchedule.ShiftsForCreate(req.Recurrence, req.ApplicableShifts);

        var template = new ResponsibilityTemplate
        {
            Title = req.Title.Trim(),
            StartDate = day,
            IsActive = true,
            Recurrence = req.Recurrence,
            ApplicableShifts = applicableShifts
        };

        _db.ResponsibilityTemplates.Add(template);
        await _db.SaveChangesAsync();

        var shiftsToCreate = ResponsibilitySchedule.MaskToShifts(applicableShifts)
            .Where(s => ResponsibilitySchedule.TemplateAppliesTo(
                day, s, req.Recurrence, applicableShifts, day))
            .ToList();

        Responsibility? firstCreated = null;

        foreach (var shift in shiftsToCreate)
        {
            var exists = await _db.Responsibilities.AnyAsync(r =>
                r.TemplateId == template.Id && r.TaskDate == day && r.Shift == shift);
            if (exists) continue;

            var nextSort = await _db.Responsibilities
                .Where(r => r.TaskDate == day && r.Shift == shift)
                .Select(r => (int?)r.SortOrder)
                .MaxAsync() ?? 0;

            var entity = new Responsibility
            {
                TemplateId = template.Id,
                Title = template.Title,
                TaskDate = day,
                Shift = shift,
                UserId = null,
                IsCompleted = false,
                SortOrder = nextSort + 1
            };

            _db.Responsibilities.Add(entity);
            firstCreated ??= entity;
        }

        await _db.SaveChangesAsync();

        if (firstCreated is null)
        {
            var fallbackShift = ResponsibilitySchedule.MaskToSingleShift(applicableShifts)
                ?? ShiftType.Morgen;
            firstCreated = new Responsibility
            {
                TemplateId = template.Id,
                Title = template.Title,
                TaskDate = day,
                Shift = fallbackShift,
                SortOrder = 0
            };
        }

        return ToDto(firstCreated, template.Recurrence, template.ApplicableShifts);
    }

    public async Task<bool> UpdateAsync(int id, ResponsibilityDTO.UpdateResponsibilityRequest req)
    {
        var entity = await _db.Responsibilities.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return false;

        entity.Title = req.Title;
        entity.UserId = req.UserId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetCompletedAsync(int id, ResponsibilityDTO.SetCompletedRequest req)
    {
        var entity = await _db.Responsibilities.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return false;

        entity.IsCompleted = req.IsCompleted;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoveAsync(int id, ResponsibilityDTO.MoveRequest req)
    {
        var entity = await _db.Responsibilities.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return false;

        var day = entity.TaskDate.Date;
        var shift = entity.Shift;

        Responsibility? neighbor = req.Direction == ResponsibilityDTO.MoveDirection.Up
            ? await _db.Responsibilities
                .Where(r => r.TaskDate == day && r.Shift == shift && r.SortOrder < entity.SortOrder)
                .OrderByDescending(r => r.SortOrder)
                .FirstOrDefaultAsync()
            : await _db.Responsibilities
                .Where(r => r.TaskDate == day && r.Shift == shift && r.SortOrder > entity.SortOrder)
                .OrderBy(r => r.SortOrder)
                .FirstOrDefaultAsync();

        if (neighbor is null) return true;

        (entity.SortOrder, neighbor.SortOrder) = (neighbor.SortOrder, entity.SortOrder);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Responsibilities.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return false;

        _db.Responsibilities.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ResponsibilityDTO.ResponsibilityDto ToDto(
        Responsibility r,
        ResponsibilityRecurrence recurrence,
        ShiftMask applicableShifts) =>
        new(
            r.Id,
            r.TemplateId,
            r.Title,
            r.SortOrder,
            r.TaskDate,
            r.Shift,
            r.UserId,
            r.IsCompleted,
            recurrence,
            applicableShifts
        );
}
