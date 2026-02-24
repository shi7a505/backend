using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        var profile = await _profileService.GetMyProfileAsync(userId);

        if (profile is null) return NotFound(new { message = "Profile not found." });

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpsertMe([FromBody] UpsertProfileRequestDto dto)
    {
        var userId = GetUserId();
        var profile = await _profileService.UpsertMyProfileAsync(userId, dto);
        return Ok(profile);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        var userId = GetUserId();
        await _profileService.SoftDeleteMyProfileAsync(userId);
        return NoContent();
    }

    private int GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var userId))
            throw new UnauthorizedAccessException("Invalid token: missing user id claim.");

        return userId;
    }
}