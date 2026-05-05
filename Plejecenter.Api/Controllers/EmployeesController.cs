using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Plejecenter.Application.Services.Employees;
using Plejecenter.Shared.DTOs.EmployePage;

namespace Plejecenter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Kræver at brugeren er logget ind (har en gyldig JWT).
    // "Write" endpoints længere nede kræver derudover Administrator-rolle.
    //[Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employees;

        public EmployeesController(IEmployeeService employees)
        {
            _employees = employees;
        }

        // GET: api/Employees?Search=...
        [HttpGet]
        public async Task<ActionResult<List<EmployeePageDTO.EmployeeDto>>> GetAll([FromQuery] string? search = null)
        {
            var items = await _employees.GetAllAsync(search);
            return Ok(items);
        }

        // GET: /api/employees/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeePageDTO.EmployeeDto>> GetById(int id)
        {
            var dto = await _employees.GetByIdAsync(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        // POST: /api/employees
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<EmployeePageDTO.EmployeeDto>> Create([FromBody] EmployeePageDTO.CreateEmployeeRequest req)
        {
            var dto = await _employees.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // PUT: /api/employees/5
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeePageDTO.UpdateEmployeeRequest req)
        {
            var ok = await _employees.UpdateAsync(id, req);
            return ok ? NoContent() : NotFound();
        }

        // PATCH: /api/employees/5/active
        [Authorize(Roles = "Administrator")]
        [HttpPatch("{id:int}/active")]
        public async Task<IActionResult> SetActive(int id, [FromBody] EmployeePageDTO.SetEmployeeActiveRequest req)
        {
            var ok = await _employees.SetActiveAsync(id, req);
            return ok ? NoContent() : NotFound();
        }

        // DELETE: /api/employees/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _employees.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

    }
        
}
