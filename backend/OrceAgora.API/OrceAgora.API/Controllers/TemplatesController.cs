using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.DTOs.Templates;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController(IServiceTemplateService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId) =>
        Ok(await service.GetAllAsync(UserId, categoryId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTemplateDto dto)
    {
        var result = await service.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTemplateDto dto)
    {
        var result = await service.UpdateAsync(id, UserId, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var result = await service.DuplicateAsync(id, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await service.DeleteAsync(id, UserId);
        return success ? NoContent() : NotFound();
    }
}