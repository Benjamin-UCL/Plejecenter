using Plejecenter.Shared.DTOs.DisplayPage;
using Plejecenter.Shared.DTOs.ResidentAdminPage;


namespace Plejecenter.Application.Services.Residents;

public class ResidentService: IResidentService
{
    private readonly IResidentRepository _repo;

    public ResidentService(IResidentRepository repo)
    {
        _repo = repo;
    }
    public Task<List<ResidentDisplayDTO>> GetAllForDisplayAsync()
    => _repo.GetAllForDisplayAsync();

    public Task<List<ResidentAdminPageDTO.ResidentDto>> GetAllAsync(string? search)
        => _repo.GetAllAsync(search);

    public Task<ResidentAdminPageDTO.ResidentDto?> GetByIdAsync(int id)
        => _repo.GetByIdAsync(id);

    public Task<ResidentAdminPageDTO.ResidentDto> CreateAsync(ResidentAdminPageDTO.CreateResidentRequest req)
        => _repo.CreateAsync(req);

    public Task<bool> UpdateAsync(int id, ResidentAdminPageDTO.UpdateResidentRequest req)
        => _repo.UpdateAsync(id, req);

    public Task<bool> SetActiveAsync(int id, ResidentAdminPageDTO.SetResidentActiveRequest req)
        => _repo.SetActiveAsync(id, req);

    public Task<bool> DeleteAsync(int id)
        => _repo.DeleteAsync(id);
}