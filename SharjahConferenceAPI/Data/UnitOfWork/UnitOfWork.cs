namespace Data.UnitOfWork;

using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Repositories;
using Data.Entities;

public class UnitOfWork : IUnitOfWork
{
    private readonly EventDbContext _context;
    
    private IRepository<Venue>? _venues;
    private IRepository<Zone>? _zones;
    private IRepository<MeetingTable>? _meetingTables;
    private IRepository<Publisher>? _publishers;
    private IRepository<Participant>? _participants;
    private IRepository<Meeting>? _meetings;
    private IRepository<Facility>? _facilities;
    private IRepository<Announcement>? _announcements;
    private IRepository<User>? _users;

    public UnitOfWork(EventDbContext context)
    {
        _context = context;
    }

    public IRepository<Venue> Venues => _venues ??= new Repository<Venue>(_context);
    public IRepository<Zone> Zones => _zones ??= new Repository<Zone>(_context);
    public IRepository<MeetingTable> MeetingTables => _meetingTables ??= new Repository<MeetingTable>(_context);
    public IRepository<Publisher> Publishers => _publishers ??= new Repository<Publisher>(_context);
    public IRepository<Participant> Participants => _participants ??= new Repository<Participant>(_context);
    public IRepository<Meeting> Meetings => _meetings ??= new Repository<Meeting>(_context);
    public IRepository<Facility> Facilities => _facilities ??= new Repository<Facility>(_context);
    public IRepository<Announcement> Announcements => _announcements ??= new Repository<Announcement>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
