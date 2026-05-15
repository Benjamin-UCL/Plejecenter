using System;
using Plejecenter.Shared.DTOs.ResponsibilityPage;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Application.Services.Responsibilities;

public class ResponsibilityService : IResponsibilityService
{
    private readonly IResponsibilityRepository _repo;

    public ResponsibilityService(IResponsibilityRepository repo)
    {
        _repo = repo;
    }

    public Task<List<ResponsibilityDTO.ResponsibilityDto>> GetAllAsync(DateTime date, ShiftType shift)
        => _repo.GetAllAsync(date, shift);

    public async Task<ResponsibilityDTO.ResponsibilityDto> CreateTemplateAsync(ResponsibilityDTO.CreateTemplateRequest req)
    {
        var error = ResponsibilityTemplateValidator.ValidateCreate(req);
        if (error is not null)
            throw new ArgumentException(error);

        return await _repo.CreateTemplateAsync(req);
    }

    public Task<bool> UpdateAsync(int id, ResponsibilityDTO.UpdateResponsibilityRequest req)
        => _repo.UpdateAsync(id, req);

    public Task<bool> SetCompletedAsync(int id, ResponsibilityDTO.SetCompletedRequest req)
        => _repo.SetCompletedAsync(id, req);

    public Task<bool> MoveAsync(int id, ResponsibilityDTO.MoveRequest req)
        => _repo.MoveAsync(id, req);

    public Task<bool> DeleteAsync(int id)
        => _repo.DeleteAsync(id);
}
