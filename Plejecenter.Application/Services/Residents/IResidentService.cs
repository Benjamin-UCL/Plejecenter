using Plejecenter.Shared.DTOs.ResidentAdminPage;

namespace Plejecenter.Application.Services.Residents;

public interface IResidentService
{
    Task<List<ResidentAdminPageDTO.ResidentDto>> GetAllAsync(string? search);
    Task<ResidentAdminPageDTO.ResidentDto?> GetByIdAsync(int id);
    Task<ResidentAdminPageDTO.ResidentDto> CreateAsync(ResidentAdminPageDTO.CreateResidentRequest req);
    Task<bool> UpdateAsync(int id, ResidentAdminPageDTO.UpdateResidentRequest req);
    Task<bool> SetActiveAsync(int id, ResidentAdminPageDTO.SetResidentActiveRequest req);
    Task<bool> DeleteAsync(int id);
}