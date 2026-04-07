using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.DTOs.Auth;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(IProfileService profileService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await profileService.GetAsync(UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileDto dto)
    {
        var result = await profileService.UpdateAsync(UserId, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("logo")]
    public async Task<IActionResult> UpdateLogo(UpdateLogoDto dto)
    {
        var result = await profileService.UpdateLogoAsync(UserId, dto.LogoBase64);
        return result is null ? NotFound() : Ok(result);
    }
}