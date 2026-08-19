namespace SharjahConferenceAPI.Services.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

/// <summary>
/// Service interface for AI Chatbot interactions powered by Google AI Studio (Gemini) and Microsoft.Extensions.AI.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Processes a user chat message with conversation history and returns a grounded AI response.
    /// </summary>
    /// <param name="request">The chat request containing message and history.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI assistant response payload.</returns>
    Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request, CancellationToken cancellationToken = default);
}
