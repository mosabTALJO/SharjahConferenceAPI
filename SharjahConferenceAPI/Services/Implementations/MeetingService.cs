namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities;
using Data.Enums;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class MeetingService(IUnitOfWork unitOfWork) : IMeetingService
{
    public async Task<IEnumerable<MeetingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.Meetings.GetAllAsync(cancellationToken);
        return entities.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<MeetingDto>> GetForUserAsync(
        string role,
        Guid? publisherId,
        Guid? participantId,
        CancellationToken cancellationToken = default)
    {
        if (role == "Admin")
        {
            return await GetAllAsync(cancellationToken);
        }

        if (role == "Publisher" && publisherId.HasValue)
        {
            var entities = await unitOfWork.Meetings.FindAsync(
                m => m.PublisherId == publisherId.Value,
                cancellationToken
            );
            return entities.Select(e => e.ToDto());
        }

        if (role == "Participant" && participantId.HasValue)
        {
            var entities = await unitOfWork.Meetings.FindAsync(
                m => m.ParticipantId == participantId.Value,
                cancellationToken
            );
            return entities.Select(e => e.ToDto());
        }

        return [];
    }

    public async Task<MeetingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Meetings.GetByIdAsync(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<MeetingDto> CreateAsync(CreateMeetingDto createDto, CancellationToken cancellationToken = default)
    {
        if (createDto.StartTime >= createDto.EndTime)
        {
            throw new ArgumentException("Meeting start time must be before end time.");
        }

        // Validate table exists and is reserved by the given publisher
        var table = await unitOfWork.MeetingTables.GetByIdAsync(createDto.MeetingTableId, cancellationToken);
        if (table == null)
        {
            throw new InvalidOperationException("Meeting table not found.");
        }

        if (table.ReservedByPublisherId != createDto.PublisherId)
        {
            throw new InvalidOperationException("Meetings can only be reserved on tables reserved by the specified publisher.");
        }

        // Validate publisher exists
        var publisher = await unitOfWork.Publishers.GetByIdAsync(createDto.PublisherId, cancellationToken);
        if (publisher == null)
        {
            throw new InvalidOperationException("Publisher not found.");
        }

        // Validate participant exists
        var participant = await unitOfWork.Participants.GetByIdAsync(createDto.ParticipantId, cancellationToken);
        if (participant == null)
        {
            throw new InvalidOperationException("Participant not found.");
        }

        // Check table schedule conflicts
        var tableConflicts = await unitOfWork.Meetings.FindAsync(
            m => m.MeetingTableId == createDto.MeetingTableId
                 && m.Status != MeetingStatus.Cancelled
                 && m.StartTime < createDto.EndTime
                 && m.EndTime > createDto.StartTime,
            cancellationToken
        );
        if (tableConflicts.Any())
        {
            throw new InvalidOperationException("The meeting table already has a scheduled meeting during this time slot.");
        }

        // Check participant schedule conflicts
        var participantConflicts = await unitOfWork.Meetings.FindAsync(
            m => m.ParticipantId == createDto.ParticipantId
                 && m.Status != MeetingStatus.Cancelled
                 && m.StartTime < createDto.EndTime
                 && m.EndTime > createDto.StartTime,
            cancellationToken
        );
        if (participantConflicts.Any())
        {
            throw new InvalidOperationException("The participant already has a scheduled meeting during this time slot.");
        }

        // Check publisher schedule conflicts
        var publisherConflicts = await unitOfWork.Meetings.FindAsync(
            m => m.PublisherId == createDto.PublisherId
                 && m.Status != MeetingStatus.Cancelled
                 && m.StartTime < createDto.EndTime
                 && m.EndTime > createDto.StartTime,
            cancellationToken
        );
        if (publisherConflicts.Any())
        {
            throw new InvalidOperationException("The publisher already has a scheduled meeting during this time slot.");
        }

        var entity = createDto.ToEntityFromCreate();
        await unitOfWork.Meetings.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<MeetingDto?> UpdateAsync(Guid id, UpdateMeetingDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Meetings.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;

        if (updateDto.StartTime >= updateDto.EndTime)
        {
            throw new ArgumentException("Meeting start time must be before end time.");
        }

        // Check collision excluding current meeting
        var tableConflicts = await unitOfWork.Meetings.FindAsync(
            m => m.Id != id
                 && m.MeetingTableId == entity.MeetingTableId
                 && m.Status != MeetingStatus.Cancelled
                 && m.StartTime < updateDto.EndTime
                 && m.EndTime > updateDto.StartTime,
            cancellationToken
        );
        if (tableConflicts.Any())
        {
            throw new InvalidOperationException("The meeting table already has a scheduled meeting during this time slot.");
        }

        entity.StartTime = updateDto.StartTime;
        entity.EndTime = updateDto.EndTime;
        entity.Status = updateDto.Status;
        entity.Notes = updateDto.Notes;

        unitOfWork.Meetings.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Meetings.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        unitOfWork.Meetings.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }
}
