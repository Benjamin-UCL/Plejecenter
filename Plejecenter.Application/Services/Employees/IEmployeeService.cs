using System;
using Plejecenter.Shared.DTOs.EmployePage;

namespace Plejecenter.Application.Services.Employees;

public interface IEmployeeService
{
    Task<List<EmployeePageDTO.EmployeeDto>> GetAllAsync(string? search); //
    Task<EmployeePageDTO.EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeePageDTO.EmployeeDto> CreateAsync(EmployeePageDTO.CreateEmployeeRequest req);
    Task<bool> UpdateAsync(int id, EmployeePageDTO.UpdateEmployeeRequest req);
    Task<bool> SetActiveAsync(int id, EmployeePageDTO.SetEmployeeActiveRequest req);
    Task<bool> DeleteAsync(int id);
}
