namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.LoginAsync(dto, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("register-participant")]
    public async Task<IActionResult> RegisterParticipant([FromBody] RegisterParticipantDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterParticipantAsync(dto, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register-publisher")]
    public async Task<IActionResult> RegisterPublisher([FromBody] RegisterPublisherDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterPublisherAsync(dto, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterAdminAsync(dto, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var profile = await authService.GetCurrentUserAsync(User, cancellationToken);
        if (profile == null)
        {
            return NotFound(new { message = "User profile not found." });
        }
        return Ok(profile);
    }
}
