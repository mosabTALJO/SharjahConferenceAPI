namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

/// <summary>
/// Implementation of the AI Tool / Function Calling service layer.
/// Provides read-only, optimized queries for LLM-driven deterministic tools.
/// </summary>
public class ConferenceAiToolsService(EventDbContext dbContext) : IConferenceAiToolsService
{
    /// <inheritdoc />
    [Description("Fuzzy searches conference participants and publishers by name, job title, publisher/organization name, or country code. Returns matching directory entries including assigned table and zone.")]
    public async Task<IReadOnlyList<DirectorySearchResultDto>> SearchDirectoryAsync(
        [Description("The search keyword or name to search for (e.g. participant name, publisher company, country code, or job title).")]
        string query,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var trimmedQuery = query.Trim();
        var pattern = $"%{trimmedQuery}%";
        var results = new List<DirectorySearchResultDto>();

        // 1. Search Participants (with non-cancelled scheduled meetings to get table/zone)
        var participants = await dbContext.Participants
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.FirstName, pattern)
                     || EF.Functions.ILike(p.LastName, pattern)
                     || EF.Functions.ILike(p.FirstName + " " + p.LastName, pattern)
                     || EF.Functions.ILike(p.Title, pattern))
            .Select(p => new
            {
                FullName = p.FirstName + " " + p.LastName,
                p.Title,
                UpcomingMeeting = p.Meetings
                    .Where(m => m.Status != MeetingStatus.Cancelled)
                    .OrderBy(m => m.StartTime)
                    .Select(m => new
                    {
                        PublisherName = m.Publisher != null ? m.Publisher.NameEn : null,
                        TableNumber = m.MeetingTable != null ? m.MeetingTable.TableNumber : null,
                        ZoneCode = m.MeetingTable != null && m.MeetingTable.Zone != null ? m.MeetingTable.Zone.Code : null
                    })
                    .FirstOrDefault()
            })
            .Take(15)
            .ToListAsync(ct);

        foreach (var p in participants)
        {
            results.Add(new DirectorySearchResultDto(
                EntryType: "Participant",
                Name: p.FullName,
                Title: p.Title,
                Organization: p.UpcomingMeeting?.PublisherName,
                Country: null,
                AssignedZone: p.UpcomingMeeting?.ZoneCode,
                AssignedTable: p.UpcomingMeeting?.TableNumber != null ? $"Table {p.UpcomingMeeting.TableNumber}" : null,
                AdditionalInfo: p.UpcomingMeeting != null
                    ? $"Scheduled meeting with {p.UpcomingMeeting.PublisherName} at Table {p.UpcomingMeeting.TableNumber}"
                    : null
            ));
        }

        // 2. Search Publishers (with reserved tables & zones)
        var publishers = await dbContext.Publishers
            .AsNoTracking()
            .Where(pub => EF.Functions.ILike(pub.NameEn, pattern)
                       || EF.Functions.ILike(pub.NameAr, pattern)
                       || EF.Functions.ILike(pub.CountryCode, pattern))
            .Select(pub => new
            {
                pub.NameEn,
                pub.NameAr,
                pub.CountryCode,
                ReservedTables = pub.ReservedTables
                    .Select(t => new
                    {
                        t.TableNumber,
                        ZoneCode = t.Zone != null ? t.Zone.Code : null
                    })
                    .ToList()
            })
            .Take(15)
            .ToListAsync(ct);

        foreach (var pub in publishers)
        {
            var primaryTable = pub.ReservedTables.FirstOrDefault();
            var tableSummary = pub.ReservedTables.Count switch
            {
                0 => null,
                1 => $"Table {primaryTable!.TableNumber}",
                _ => string.Join(", ", pub.ReservedTables.Select(t => $"Table {t.TableNumber}"))
            };
            var zoneSummary = primaryTable?.ZoneCode;

            results.Add(new DirectorySearchResultDto(
                EntryType: "Publisher",
                Name: pub.NameEn,
                Title: pub.NameAr != pub.NameEn ? pub.NameAr : null,
                Organization: pub.NameEn,
                Country: pub.CountryCode,
                AssignedZone: zoneSummary,
                AssignedTable: tableSummary,
                AdditionalInfo: pub.ReservedTables.Count > 0
                    ? $"Reserved table(s): {tableSummary}"
                    : "No dedicated table reserved"
            ));
        }

        return results;
    }

    /// <inheritdoc />
    [Description("Retrieves the table's zone, coordinates, current status (Available, Occupied, Reserved), reserving publisher, and the next scheduled meeting time.")]
    public async Task<TableStatusResultDto?> GetTableStatusAsync(
        [Description("The integer number of the meeting table to inspect (e.g. 1, 5, 20).")]
        int tableNumber,
        CancellationToken ct = default)
    {
        var tableNumStr = tableNumber.ToString();
        var now = DateTime.UtcNow;

        var table = await dbContext.MeetingTables
            .AsNoTracking()
            .Where(t => t.TableNumber == tableNumStr
                     || t.TableNumber == $"Table {tableNumStr}"
                     || t.TableNumber == $"T-{tableNumStr}"
                     || t.TableNumber == tableNumber.ToString("D2"))
            .Select(t => new
            {
                t.TableNumber,
                ZoneCode = t.Zone != null ? t.Zone.Code : "N/A",
                ZoneName = t.Zone != null ? t.Zone.NameEn : "N/A",
                t.CoordX,
                t.CoordY,
                t.Capacity,
                t.OperationalStatus,
                ReservedByPublisher = t.ReservedByPublisher != null ? t.ReservedByPublisher.NameEn : null,
                CurrentMeeting = t.Meetings
                    .Where(m => m.Status != MeetingStatus.Cancelled && m.StartTime <= now && m.EndTime > now)
                    .Select(m => new
                    {
                        m.StartTime,
                        m.EndTime,
                        PublisherName = m.Publisher != null ? m.Publisher.NameEn : null,
                        ParticipantName = m.Participant != null ? m.Participant.FirstName + " " + m.Participant.LastName : null
                    })
                    .FirstOrDefault(),
                NextMeeting = t.Meetings
                    .Where(m => m.Status != MeetingStatus.Cancelled && m.StartTime > now)
                    .OrderBy(m => m.StartTime)
                    .Select(m => new
                    {
                        m.StartTime,
                        m.EndTime,
                        PublisherName = m.Publisher != null ? m.Publisher.NameEn : null,
                        ParticipantName = m.Participant != null ? m.Participant.FirstName + " " + m.Participant.LastName : null
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (table == null)
        {
            return null;
        }

        // Compute real-time operational status
        string currentStatus;
        if (table.CurrentMeeting != null || table.OperationalStatus == TableStatus.Occupied)
        {
            currentStatus = "Occupied";
        }
        else if (table.OperationalStatus == TableStatus.Reserved || !string.IsNullOrEmpty(table.ReservedByPublisher))
        {
            currentStatus = "Reserved";
        }
        else
        {
            currentStatus = "Available";
        }

        return new TableStatusResultDto(
            TableNumber: table.TableNumber,
            ZoneCode: table.ZoneCode,
            ZoneName: table.ZoneName,
            CoordX: table.CoordX,
            CoordY: table.CoordY,
            Capacity: table.Capacity,
            Status: currentStatus,
            ReservedByPublisher: table.ReservedByPublisher,
            CurrentMeetingStartTime: table.CurrentMeeting?.StartTime,
            CurrentMeetingEndTime: table.CurrentMeeting?.EndTime,
            NextScheduledMeetingStartTime: table.NextMeeting?.StartTime,
            NextScheduledMeetingEndTime: table.NextMeeting?.EndTime,
            NextMeetingPublisherName: table.NextMeeting?.PublisherName,
            NextMeetingParticipantName: table.NextMeeting?.ParticipantName
        );
    }

    /// <inheritdoc />
    [Description("Looks up scheduled meetings for an individual participant by their name, including start/end times, status, table numbers, and zone locations.")]
    public async Task<IReadOnlyList<ParticipantScheduleResultDto>> GetParticipantScheduleAsync(
        [Description("The full or partial name of the conference participant (e.g. 'Jane Doe', 'Ahmed').")]
        string participantName,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(participantName))
        {
            return [];
        }

        var pattern = $"%{participantName.Trim()}%";

        var participants = await dbContext.Participants
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.FirstName, pattern)
                     || EF.Functions.ILike(p.LastName, pattern)
                     || EF.Functions.ILike(p.FirstName + " " + p.LastName, pattern))
            .Select(p => new
            {
                FullName = p.FirstName + " " + p.LastName,
                p.Title,
                Meetings = p.Meetings
                    .Where(m => m.Status != MeetingStatus.Cancelled)
                    .OrderBy(m => m.StartTime)
                    .Select(m => new ParticipantMeetingScheduleDto(
                        m.Id,
                        m.StartTime,
                        m.EndTime,
                        m.Status.ToString(),
                        m.MeetingTable != null ? m.MeetingTable.TableNumber : "Unassigned",
                        m.MeetingTable != null && m.MeetingTable.Zone != null ? m.MeetingTable.Zone.Code : null,
                        m.MeetingTable != null && m.MeetingTable.Zone != null ? m.MeetingTable.Zone.NameEn : null,
                        m.Publisher != null ? m.Publisher.NameEn : "Unspecified Publisher",
                        m.Notes
                    ))
                    .ToList()
            })
            .Take(10)
            .ToListAsync(ct);

        return participants.Select(p => new ParticipantScheduleResultDto(
            ParticipantName: p.FullName,
            Title: p.Title,
            ScheduledMeetings: p.Meetings
        )).ToList();
    }

    /// <inheritdoc />
    [Description("Finds points of interest and facilities (e.g., 'Registration Desk', 'Media Center', 'Prayer Room', 'Cafeteria', 'Restrooms') with their zone and coordinate data.")]
    public async Task<IReadOnlyList<FacilityLocationDto>> GetFacilityLocationAsync(
        [Description("The name or category of the facility/point of interest (e.g., 'Registration', 'Prayer Room', 'Media Center').")]
        string facilityNameOrCategory,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(facilityNameOrCategory))
        {
            return [];
        }

        var pattern = $"%{facilityNameOrCategory.Trim()}%";

        var facilities = await dbContext.Facilities
            .AsNoTracking()
            .Where(f => EF.Functions.ILike(f.Name, pattern) || EF.Functions.ILike(f.Category, pattern))
            .Take(15)
            .ToListAsync(ct);

        if (facilities.Count == 0)
        {
            return [];
        }

        // Fetch zones with table coordinates to compute nearest zone context
        var zonesWithCoordinates = await dbContext.Zones
            .AsNoTracking()
            .Select(z => new
            {
                z.Code,
                z.NameEn,
                AvgX = z.Tables.Any() ? z.Tables.Average(t => t.CoordX) : (double?)null,
                AvgY = z.Tables.Any() ? z.Tables.Average(t => t.CoordY) : (double?)null
            })
            .Where(z => z.AvgX.HasValue && z.AvgY.HasValue)
            .ToListAsync(ct);

        var result = new List<FacilityLocationDto>(facilities.Count);

        foreach (var f in facilities)
        {
            string? nearestZoneCode = null;
            string? nearestZoneName = null;

            if (zonesWithCoordinates.Count > 0)
            {
                var closestZone = zonesWithCoordinates
                    .OrderBy(z => Math.Pow(f.CoordX - z.AvgX!.Value, 2) + Math.Pow(f.CoordY - z.AvgY!.Value, 2))
                    .FirstOrDefault();

                if (closestZone != null)
                {
                    nearestZoneCode = closestZone.Code;
                    nearestZoneName = closestZone.NameEn;
                }
            }

            result.Add(new FacilityLocationDto(
                Id: f.Id,
                Name: f.Name,
                Category: f.Category,
                CoordX: f.CoordX,
                CoordY: f.CoordY,
                NearestZoneCode: nearestZoneCode,
                NearestZoneName: nearestZoneName
            ));
        }

        return result;
    }

    /// <inheritdoc />
    [Description("Retrieves current high-priority public announcements that are currently active for conference attendees.")]
    public async Task<IReadOnlyList<ActiveAnnouncementDto>> GetActiveAnnouncementsAsync(
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Primary: fetch active High and Urgent priority announcements
        var highPriority = await dbContext.Announcements
            .AsNoTracking()
            .Where(a => a.ActiveStartTime <= now && a.ActiveEndTime >= now
                     && (a.Priority == AnnouncementPriority.High || a.Priority == AnnouncementPriority.Urgent))
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.ActiveStartTime)
            .Select(a => new ActiveAnnouncementDto(
                a.Id,
                a.Title,
                a.Body,
                a.Priority.ToString(),
                a.ActiveStartTime,
                a.ActiveEndTime
            ))
            .ToListAsync(ct);

        if (highPriority.Count > 0)
        {
            return highPriority;
        }

        // Fallback: return any active announcements ordered by priority
        return await dbContext.Announcements
            .AsNoTracking()
            .Where(a => a.ActiveStartTime <= now && a.ActiveEndTime >= now)
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.ActiveStartTime)
            .Select(a => new ActiveAnnouncementDto(
                a.Id,
                a.Title,
                a.Body,
                a.Priority.ToString(),
                a.ActiveStartTime,
                a.ActiveEndTime
            ))
            .ToListAsync(ct);
    }
}
