using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.DTOs.Subscription;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController(ISubscriptionService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus() =>
        Ok(await service.GetStatusAsync(UserId));

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade(UpgradeDto dto)
    {
        var subscriptionId = await service.UpgradeToProAsync(UserId, dto);
        return Ok(new { subscriptionId, message = "Assinatura Pro ativada!" });
    }

    [HttpDelete("cancel")]
    public async Task<IActionResult> Cancel()
    {
        await service.CancelProAsync(UserId);
        return Ok(new { message = "Assinatura cancelada." });
    }

    // Webhook do Asaas — sem autenticação
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var payload = await new StreamReader(Request.Body)
            .ReadToEndAsync();
        await service.HandleWebhookAsync(payload);
        return Ok();
    }
}