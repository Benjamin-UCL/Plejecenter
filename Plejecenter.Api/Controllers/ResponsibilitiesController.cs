using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plejecenter.Application.Services.Responsibilities;
using Plejecenter.Shared.DTOs.ResponsibilityPage;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/responsibilities")]
public class ResponsibilitiesController : ControllerBase
{
    private readonly IResponsibilityService _responsibilities;

    public ResponsibilitiesController(IResponsibilityService responsibilities)
    {
        _responsibilities = responsibilities;
    }

    // GET /api/responsibilities?date=2026-05-07&shift=Morgen
    [HttpGet]
    public async Task<ActionResult<List<ResponsibilityDTO.ResponsibilityDto>>> GetAll(
        [FromQuery] DateTime date,
        [FromQuery] ShiftType shift)
    {
        var items = await _responsibilities.GetAllAsync(date, shift);
        return Ok(items);
    }

    // POST /api/responsibilities
    [HttpPost]
    public async Task<ActionResult<ResponsibilityDTO.ResponsibilityDto>> Create(
        [FromBody] ResponsibilityDTO.CreateTemplateRequest req)
    {
        var dto = await _responsibilities.CreateTemplateAsync(req);
        return CreatedAtAction(nameof(GetAll), new { date = dto.TaskDate.Date, shift = dto.Shift }, dto);
    }

    // PUT /api/responsibilities/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ResponsibilityDTO.UpdateResponsibilityRequest req)
    {
        var ok = await _responsibilities.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    // PATCH /api/responsibilities/{id}/completed
    [HttpPatch("{id:int}/completed")]
    public async Task<IActionResult> SetCompleted(int id, [FromBody] ResponsibilityDTO.SetCompletedRequest req)
    {
        var ok = await _responsibilities.SetCompletedAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    // POST /api/responsibilities/{id}/move
    [HttpPost("{id:int}/move")]
    public async Task<IActionResult> Move(int id, [FromBody] ResponsibilityDTO.MoveRequest req)
    {
        var ok = await _responsibilities.MoveAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    // DELETE /api/responsibilities/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _responsibilities.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
