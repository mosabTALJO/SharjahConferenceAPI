namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

/// <summary>
/// Controller for AI Chatbot interactions.
/// Allows authenticated conference attendees, publishers, and admins to converse with the conference AI assistant.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController(IChatService chatService) : ControllerBase
{
    /// <summary>
    /// Processes a chat request and returns the AI assistant's grounded response.
    /// </summary>
    /// <param name="request">The chat message payload and optional conversation history.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The synthesized AI response payload.</returns>
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Chat message is required and cannot be empty." });
        }

        try
        {
            var response = await chatService.SendMessageAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while communicating with the AI service.", details = ex.Message });
        }
    }
}
