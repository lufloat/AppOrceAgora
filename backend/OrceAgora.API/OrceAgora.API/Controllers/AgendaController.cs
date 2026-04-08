using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.DTOs.Agenda;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgendaController(IAgendaService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? done) =>
        Ok(await service.GetAllAsync(UserId, done));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAgendaEventDto dto)
    {
        var result = await service.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateAgendaEventDto dto)
    {
        var result = await service.UpdateAsync(id, UserId, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var result = await service.ToggleDoneAsync(id, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await service.DeleteAsync(id, UserId);
        return success ? NoContent() : NotFound();
    }
}