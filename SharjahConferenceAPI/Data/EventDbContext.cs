namespace Data;

using Microsoft.EntityFrameworkCore;
using Data.Entities;
using Data.Enums;
using Npgsql;

public class EventDbContext : DbContext
{
    public EventDbContext()
    {
    }

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<MeetingTable> MeetingTables => Set<MeetingTable>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=ep-little-pond-b2njobgt.c-6.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_eHx2qM7ioGJQ;Ssl Mode=Require;");
            
            // Npgsql 7.0+ requires mapping enums at the ADO.NET connection layer as well
            dataSourceBuilder.MapEnum<TableStatus>();
            dataSourceBuilder.MapEnum<MeetingStatus>();
            dataSourceBuilder.MapEnum<AnnouncementPriority>();
            dataSourceBuilder.MapEnum<UserRole>();
            
            optionsBuilder.UseNpgsql(dataSourceBuilder.Build());
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map PostgreSQL native enums
        modelBuilder.HasPostgresEnum<TableStatus>();
        modelBuilder.HasPostgresEnum<MeetingStatus>();
        modelBuilder.HasPostgresEnum<AnnouncementPriority>();
        modelBuilder.HasPostgresEnum<UserRole>();

        // Venue Configuration
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameEn).HasMaxLength(255).IsRequired();
            entity.Property(e => e.NameAr).HasMaxLength(255).IsRequired();
            
            entity.HasMany(e => e.Zones)
                  .WithOne(e => e.Venue)
                  .HasForeignKey(e => e.VenueId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Zone Configuration
        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique(); // Ensure Zone Code is unique
            
            entity.Property(e => e.NameEn).HasMaxLength(255).IsRequired();
            entity.Property(e => e.NameAr).HasMaxLength(255).IsRequired();
            
            // Map the SVG Bounds string to a native jsonb column
            entity.Property(e => e.SvgBounds).HasColumnType("jsonb").IsRequired();

            entity.HasMany(e => e.Tables)
                  .WithOne(e => e.Zone)
                  .HasForeignKey(e => e.ZoneId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MeetingTable Configuration
        modelBuilder.Entity<MeetingTable>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.TableNumber).HasMaxLength(50).IsRequired();
            // TableNumber is typically unique per Zone
            entity.HasIndex(e => new { e.ZoneId, e.TableNumber }).IsUnique();
            
            entity.HasMany(e => e.Meetings)
                  .WithOne(e => e.MeetingTable)
                  .HasForeignKey(e => e.MeetingTableId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReservedByPublisher)
                  .WithMany(p => p.ReservedTables)
                  .HasForeignKey(e => e.ReservedByPublisherId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Publisher Configuration
        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameEn).HasMaxLength(255).IsRequired();
            entity.Property(e => e.NameAr).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CountryCode).HasMaxLength(3).IsRequired();

            entity.HasMany(e => e.Meetings)
                  .WithOne(e => e.Publisher)
                  .HasForeignKey(e => e.PublisherId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Participant Configuration
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique(); // Ensure Email is unique
            
            entity.Property(e => e.Phone).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();

            entity.HasMany(e => e.Meetings)
                  .WithOne(e => e.Participant)
                  .HasForeignKey(e => e.ParticipantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Publisher)
                  .WithOne(p => p.User)
                  .HasForeignKey<User>(e => e.PublisherId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Participant)
                  .WithOne(p => p.User)
                  .HasForeignKey<User>(e => e.ParticipantId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Meeting Configuration
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            // Add indexes for frequent time-based and status-based querying
            entity.HasIndex(e => new { e.MeetingTableId, e.StartTime, e.EndTime });
            entity.HasIndex(e => new { e.PublisherId, e.StartTime, e.EndTime });
            entity.HasIndex(e => new { e.ParticipantId, e.StartTime, e.EndTime });
        });

        // Facility Configuration
        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        // Announcement Configuration
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            // Body is unlimited text (PostgreSQL native `text`)
        });
    }
}
