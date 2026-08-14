using Data;
using Data.Repositories;
using Data.UnitOfWork;
using SharjahConferenceAPI.Services.Interfaces;
using SharjahConferenceAPI.Services.Implementations;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register DbContext and Data Access Layer
builder.Services.AddDbContext<EventDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Business Logic Services
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<IMeetingTableService, MeetingTableService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
