namespace Data.UnitOfWork;

using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Repositories;
using Data.Entities;

public interface IUnitOfWork : IDisposable
{
    IRepository<Venue> Venues { get; }
    IRepository<Zone> Zones { get; }
    IRepository<MeetingTable> MeetingTables { get; }
    IRepository<Publisher> Publishers { get; }
    IRepository<Participant> Participants { get; }
    IRepository<Meeting> Meetings { get; }
    IRepository<Facility> Facilities { get; }
    IRepository<Announcement> Announcements { get; }

    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
