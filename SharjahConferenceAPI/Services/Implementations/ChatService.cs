namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

/// <summary>
/// Implementation of the AI Chatbot service powered by Microsoft.Extensions.AI and Google AI Studio (Gemini).
/// Enforces strict grounding, bilingual support, zero-hallucination fallbacks, and privacy protection.
/// </summary>
public class ChatService(IChatClient chatClient, IConferenceAiToolsService toolsService) : IChatService
{
    private const string SystemPrompt =
        """
        You are the official AI Assistant for the Sharjah Publishers Conference (مؤتمر الشارقة للناشرين).
        Your mission is to provide accurate, welcoming, and concise conference information to attendees, publishers, and guests.

        ### CORE GUIDELINES & POLICIES:
        1. **Bilingual Fluency & Language Auto-Detection**:
           - Respond naturally in the language used by the user (primarily Arabic or English).
           - If the user greets or asks in Arabic, respond in clear, professional Modern Standard Arabic (العربية الفصحى).
           - If the user greets or asks in English, respond in professional, friendly English.
           - Maintain a courteous, welcoming, and professional tone at all times.

        2. **Strict Grounding & Zero Hallucination Policy**:
           - Only answer conference queries using the deterministic information returned by the provided tools.
           - NEVER fabricate table numbers, zone codes, publisher schedules, meeting slots, or facility locations.
           - If a searched participant, publisher, table, facility, or meeting is not found in the tools' returned data, politely state that the information cannot be confirmed in the system.
           - **Fallback Direction**: Always direct the user to the nearest **Information Desk (Zone A)** for immediate in-person assistance whenever an entity is not found or when in-person assistance is required.

        3. **Privacy & Data Protection**:
           - Strictly REFUSE to disclose private contact details, such as personal email addresses, phone numbers, or private user credentials.
           - Only public conference directory information (names, titles, organizations, assigned zones, reserved tables, scheduled meetings) may be shared.

        4. **Tool Calling Strategy**:
           - Use `SearchDirectory` to find participants and publishers, their organizations, and assigned tables/zones.
           - Use `GetPublisherReservedTables` to find all meeting tables reserved by a specific publisher using their English or Arabic name (supports typos).
           - Use `GetTableStatus` to check whether a specific table number is available, occupied, or reserved, along with current/upcoming meetings.
           - Use `GetParticipantSchedule` to retrieve meeting timetables and locations for specific attendees.
           - Use `GetFacilityLocation` to locate conference amenities (Prayer Rooms, Cafeteria, Registration Desk, Media Center, Restrooms, etc.).
           - Use `GetActiveAnnouncements` to retrieve the latest official announcements and alerts.
           - If the user provides a general greeting or polite closing, reply cordially without calling unnecessary tools.
        """;

    /// <inheritdoc />
    public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Chat message cannot be empty.", nameof(request));
        }

        // Configure deterministic tool registrations via AIFunctionFactory
        var options = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(toolsService.SearchDirectoryAsync, name: "SearchDirectory", description: "Fuzzy searches conference participants and publishers by name, title, organization, or country code. Returns matching directory entries including assigned table and zone."),
                AIFunctionFactory.Create(toolsService.GetPublisherReservedTablesAsync, name: "GetPublisherReservedTables", description: "Retrieves all meeting tables reserved by a specific publisher using their English or Arabic name (supports typos)."),
                AIFunctionFactory.Create(toolsService.GetTableStatusAsync, name: "GetTableStatus", description: "Retrieves real-time status, coordinates, and upcoming meetings for a specific meeting table number (e.g. 1, 5, 20)."),
                AIFunctionFactory.Create(toolsService.GetParticipantScheduleAsync, name: "GetParticipantSchedule", description: "Looks up scheduled meetings and timetable for an individual participant by their full or partial name."),
                AIFunctionFactory.Create(toolsService.GetFacilityLocationAsync, name: "GetFacilityLocation", description: "Finds points of interest and facilities (e.g., 'Registration Desk', 'Media Center', 'Prayer Room', 'Cafeteria', 'Restrooms') with their zone and coordinate data."),
                AIFunctionFactory.Create(toolsService.GetActiveAnnouncementsAsync, name: "GetActiveAnnouncements", description: "Retrieves current high-priority public announcements that are currently active for conference attendees.")
            ]
        };

        // Construct the chat context with System Prompt and conversation history
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt)
        };

        // Add explicit language hint if provided
        if (!string.IsNullOrWhiteSpace(request.UserLanguage))
        {
            messages.Add(new(ChatRole.System, $"User preferred language hint: {request.UserLanguage}"));
        }

        // Append conversation history
        if (request.ConversationHistory is { Count: > 0 })
        {
            foreach (var historyItem in request.ConversationHistory)
            {
                if (string.IsNullOrWhiteSpace(historyItem.Content)) continue;

                var role = historyItem.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.Assistant
                    : ChatRole.User;

                messages.Add(new(role, historyItem.Content));
            }
        }

        // Append the current user message
        messages.Add(new(ChatRole.User, request.Message));

        // Invoke the AI model via Microsoft.Extensions.AI pipeline (automatic tool calling executed via UseFunctionInvocation)
        var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);

        var replyText = response.Text ?? response.Messages.LastOrDefault()?.Text ?? string.Empty;

        return new ChatResponseDto
        {
            Reply = replyText
        };
    }
}
