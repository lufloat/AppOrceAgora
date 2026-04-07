using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.DTOs.Budgets;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController(IBudgetService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await service.GetAllAsync(UserId, status, page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    // Rota pública — cliente abre o link de aprovação
    [HttpGet("approve/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByToken(Guid token)
    {
        var result = await service.GetByApprovalTokenAsync(token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBudgetDto dto)
    {
        var result = await service.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var result = await service.GeneratePdfAsync(id, UserId);
        if (result is null) return NotFound();
        return File(result.Value.Pdf, "application/pdf",
            $"orcamento-{result.Value.Budget.Number:D4}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await service.DeleteAsync(id, UserId);
        return success ? NoContent() : NotFound();
    }

    // Rota pública — cliente aprova ou recusa
    [HttpPost("approve/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessApproval(Guid token, ApproveRejectDto dto)
    {
        var result = await service.ProcessApprovalAsync(token, dto);
        return result is null ? NotFound() : Ok(result);
    }
}