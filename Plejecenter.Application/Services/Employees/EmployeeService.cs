using Plejecenter.Shared.DTOs.EmployePage;


namespace Plejecenter.Application.Services.Employees;

public class EmployeeService: IEmployeeService
{
    private readonly IEmployeeRepository _repo;

    public EmployeeService(IEmployeeRepository repo)
    {
        _repo = repo;
    }

    public Task<List<EmployeePageDTO.EmployeeDto>> GetAllAsync(string? search)
        => _repo.GetAllAsync(search);

    public Task<EmployeePageDTO.EmployeeDto?> GetByIdAsync(int id)
        => _repo.GetByIdAsync(id);

    public Task<EmployeePageDTO.EmployeeDto> CreateAsync(EmployeePageDTO.CreateEmployeeRequest req)
        => _repo.CreateAsync(req);

    public Task<bool> UpdateAsync(int id, EmployeePageDTO.UpdateEmployeeRequest req)
        => _repo.UpdateAsync(id, req);

    public Task<bool> SetActiveAsync(int id, EmployeePageDTO.SetEmployeeActiveRequest req)
        => _repo.SetActiveAsync(id, req);

    public Task<bool> DeleteAsync(int id)
        => _repo.DeleteAsync(id);
}
