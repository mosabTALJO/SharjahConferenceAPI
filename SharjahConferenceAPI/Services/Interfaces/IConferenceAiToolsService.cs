namespace SharjahConferenceAPI.Services.Interfaces;

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

/// <summary>
/// AI Tool / Function Calling service layer for LLM function calling in Sharjah Publishers Conference.
/// </summary>
public interface IConferenceAiToolsService
{
    /// <summary>
    /// Fuzzy searches participants and publishers by name, title, organization, or country.
    /// </summary>
    [Description("Fuzzy searches conference participants and publishers by name, job title, publisher/organization name, or country code. Returns matching directory entries including assigned table and zone.")]
    Task<IReadOnlyList<DirectorySearchResultDto>> SearchDirectoryAsync(
        [Description("The search keyword or name to search for (e.g. participant name, publisher company, country code, or job title).")]
        string query,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves real-time status, coordinates, and upcoming meetings for a specific meeting table.
    /// </summary>
    [Description("Retrieves the table's zone, coordinates, current status (Available, Occupied, Reserved), reserving publisher, and the next scheduled meeting time.")]
    Task<TableStatusResultDto?> GetTableStatusAsync(
        [Description("The string number of the meeting table to inspect (e.g. A-017, A-020, A-001).")]
        string tableNumber,
        CancellationToken ct = default);

    /// <summary>
    /// Looks up scheduled meetings and timetable for an individual conference participant.
    /// </summary>
    [Description("Looks up scheduled meetings for an individual participant by their name, including start/end times, status, table numbers, and zone locations.")]
    Task<IReadOnlyList<ParticipantScheduleResultDto>> GetParticipantScheduleAsync(
        [Description("The full or partial name of the conference participant (e.g. 'Jane Doe', 'Ahmed').")]
        string participantName,
        CancellationToken ct = default);

    /// <summary>
    /// Finds points of interest and facility locations with zone and coordinate data.
    /// </summary>
    [Description("Finds points of interest and facilities (e.g., 'Registration Desk', 'Media Center', 'Prayer Room', 'Cafeteria', 'Restrooms') with their zone and coordinate data.")]
    Task<IReadOnlyList<FacilityLocationDto>> GetFacilityLocationAsync(
        [Description("The name or category of the facility/point of interest (e.g., 'Registration', 'Prayer Room', 'Media Center').")]
        string facilityNameOrCategory,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves current active high-priority public announcements.
    /// </summary>
    [Description("Retrieves current high-priority public announcements that are currently active for conference attendees.")]
    Task<IReadOnlyList<ActiveAnnouncementDto>> GetActiveAnnouncementsAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all meeting tables reserved by a specific publisher using their English or Arabic name, with typo tolerance.
    /// </summary>
    [Description("Retrieves all meeting tables reserved by a specific publisher using their English or Arabic name, including table numbers, zone codes, coordinates, and capacity. Supports fuzzy matching and typo tolerance.")]
    Task<IReadOnlyList<PublisherReservedTablesResultDto>> GetPublisherReservedTablesAsync(
        [Description("The English or Arabic name of the publisher (e.g. 'Obeikan Education', 'العبيكان', 'Gallimard', 'Dar Al Tanweer'). Typos are automatically handled.")]
        string publisherName,
        CancellationToken ct = default);
}

