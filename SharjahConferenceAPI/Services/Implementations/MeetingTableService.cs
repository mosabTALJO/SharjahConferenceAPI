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

public class MeetingTableService(IUnitOfWork unitOfWork) : IMeetingTableService
{
    public async Task<IEnumerable<MeetingTableDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.MeetingTables.GetAllAsync(cancellationToken, t => t.ReservedByPublisher!);
        return entities.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<MeetingTableDto>> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.MeetingTables.FindAsync(
            t => t.ZoneId == zoneId,
            cancellationToken,
            t => t.ReservedByPublisher!
        );
        return entities.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<MeetingTableDto>> GetByPublisherIdAsync(Guid publisherId, CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.MeetingTables.FindAsync(
            t => t.ReservedByPublisherId == publisherId,
            cancellationToken,
            t => t.ReservedByPublisher!
        );
        return entities.Select(e => e.ToDto());
    }

    public async Task<MeetingTableDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.MeetingTables.GetByIdAsync(id, cancellationToken, t => t.ReservedByPublisher!);
        return entity?.ToDto();
    }

    public async Task<MeetingTableDto> CreateAsync(CreateMeetingTableDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = createDto.ToEntityFromCreate();
        await unitOfWork.MeetingTables.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<MeetingTableDto?> UpdateAsync(Guid id, UpdateMeetingTableDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.MeetingTables.GetByIdAsync(id, cancellationToken, t => t.ReservedByPublisher!);
        if (entity == null) return null;

        entity.TableNumber = updateDto.TableNumber;
        entity.CoordX = updateDto.CoordX;
        entity.CoordY = updateDto.CoordY;
        entity.Capacity = updateDto.Capacity;
        entity.OperationalStatus = updateDto.OperationalStatus;

        unitOfWork.MeetingTables.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.MeetingTables.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        unitOfWork.MeetingTables.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<MeetingTableDto?> ReserveTableAsync(Guid tableId, Guid publisherId, CancellationToken cancellationToken = default)
    {
        var table = await unitOfWork.MeetingTables.GetByIdAsync(tableId, cancellationToken, t => t.ReservedByPublisher!);
        if (table == null) return null;

        if (table.OperationalStatus != TableStatus.Available || (table.ReservedByPublisherId.HasValue && table.ReservedByPublisherId != publisherId))
        {
            throw new InvalidOperationException("Table is not available for reservation.");
        }

        var publisher = await unitOfWork.Publishers.GetByIdAsync(publisherId, cancellationToken);
        if (publisher == null)
        {
            throw new InvalidOperationException("Publisher not found.");
        }

        table.OperationalStatus = TableStatus.Reserved;
        table.ReservedByPublisherId = publisherId;

        unitOfWork.MeetingTables.Update(table);
        await unitOfWork.CompleteAsync(cancellationToken);

        var refreshed = await unitOfWork.MeetingTables.GetByIdAsync(tableId, cancellationToken, t => t.ReservedByPublisher!);
        return refreshed?.ToDto() ?? table.ToDto();
    }

    public async Task<bool> UnreserveTableAsync(Guid tableId, Guid publisherId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var table = await unitOfWork.MeetingTables.GetByIdAsync(tableId, cancellationToken);
        if (table == null) return false;

        if (!isAdmin && table.ReservedByPublisherId != publisherId)
        {
            throw new UnauthorizedAccessException("You can only unreserve a table reserved by you.");
        }

        table.OperationalStatus = TableStatus.Available;
        table.ReservedByPublisherId = null;

        unitOfWork.MeetingTables.Update(table);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }
}
