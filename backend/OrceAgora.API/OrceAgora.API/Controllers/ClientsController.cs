using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.DTOs.Clients;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController(IClientService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search) =>
        Ok(await service.GetAllAsync(UserId, search));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClientDto dto)
    {
        var result = await service.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateClientDto dto)
    {
        var result = await service.UpdateAsync(id, UserId, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await service.DeleteAsync(id, UserId);
        return success ? NoContent() : NotFound();
    }
}