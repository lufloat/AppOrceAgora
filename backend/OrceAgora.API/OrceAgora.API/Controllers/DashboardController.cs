using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await service.GetDashboardAsync(UserId));
}