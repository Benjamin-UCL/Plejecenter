using Microsoft.EntityFrameworkCore;
using Plejecenter.Application.Services.Responsibilities;
using Plejecenter.Domain;
using Plejecenter.Infrastructure.Data;
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
            .Where(t => t.IsActive && t.StartDate.Date <= day)
            .OrderBy(t => t.Id)
            .ToListAsync();

        var existing = await _db.Responsibilities
            .Where(r => r.TaskDate == day && r.Shift == shift)
            .ToListAsync();

        var existingTemplateIds = existing.Select(e => e.TemplateId).ToHashSet();
        if (templates.Count > 0)
        {
            var nextSort = existing.Any() ? existing.Max(x => x.SortOrder) : 0;
            var toAdd = new List<Responsibility>();

            foreach (var t in templates)
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

        return existing
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id)
            .Select(r => new ResponsibilityDTO.ResponsibilityDto(
                r.Id,
                r.TemplateId,
                r.Title,
                r.SortOrder,
                r.TaskDate,
                r.Shift,
                r.UserId,
                r.IsCompleted
            ))
            .ToList();
    }

    public async Task<ResponsibilityDTO.ResponsibilityDto> CreateTemplateAsync(ResponsibilityDTO.CreateTemplateRequest req)
    {
        var day = req.StartDate.Date;

        var template = new ResponsibilityTemplate
        {
            Title = req.Title,
            StartDate = day,
            IsActive = true
        };

        _db.ResponsibilityTemplates.Add(template);
        await _db.SaveChangesAsync();

        var nextSort = await _db.Responsibilities
            .Where(r => r.TaskDate == day && r.Shift == req.Shift)
            .Select(r => (int?)r.SortOrder)
            .MaxAsync() ?? 0;

        var entity = new Responsibility
        {
            TemplateId = template.Id,
            Title = template.Title,
            TaskDate = day,
            Shift = req.Shift,
            UserId = null,
            IsCompleted = false,
            SortOrder = nextSort + 1
        };

        _db.Responsibilities.Add(entity);
        await _db.SaveChangesAsync();

        return new ResponsibilityDTO.ResponsibilityDto(
            Id: entity.Id,
            TemplateId: entity.TemplateId,
            Title: entity.Title,
            SortOrder: entity.SortOrder,
            TaskDate: entity.TaskDate,
            Shift: entity.Shift,
            UserId: entity.UserId,
            IsCompleted: entity.IsCompleted
        );
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
}

