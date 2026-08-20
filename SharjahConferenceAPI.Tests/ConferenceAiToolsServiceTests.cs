namespace SharjahConferenceAPI.Tests;

using System;
using System.Threading.Tasks;
using Data;
using Data.Entities;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using SharjahConferenceAPI.Services.Implementations;
using Xunit;

public class ConferenceAiToolsServiceTests
{
    private static EventDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EventDbContext(options);
    }

    private static async Task SeedSampleDataAsync(EventDbContext context)
    {
        var venue = new Venue
        {
            NameEn = "Sharjah Expo Centre",
            NameAr = "مركز إكسبو الشارقة"
        };
        context.Venues.Add(venue);

        var zoneA = new Zone
        {
            VenueId = venue.Id,
            Code = "ZONE-A",
            NameEn = "Zone A - Fiction",
            NameAr = "المنطقة أ",
            SvgBounds = "{}"
        };
        var zoneB = new Zone
        {
            VenueId = venue.Id,
            Code = "ZONE-B",
            NameEn = "Zone B - Academic",
            NameAr = "المنطقة ب",
            SvgBounds = "{}"
        };
        context.Zones.AddRange(zoneA, zoneB);

        var pubObeikan = new Publisher
        {
            NameEn = "Obeikan Education",
            NameAr = "مجموعة العبيكان للتعليم",
            CountryCode = "SAU"
        };

        var pubGallimard = new Publisher
        {
            NameEn = "Éditions Gallimard",
            NameAr = "منشورات غاليمار",
            CountryCode = "FRA"
        };

        var pubTanweer = new Publisher
        {
            NameEn = "Dar Al Tanweer",
            NameAr = "دار التنوير",
            CountryCode = "LBN"
        };

        context.Publishers.AddRange(pubObeikan, pubGallimard, pubTanweer);

        var table1 = new MeetingTable
        {
            ZoneId = zoneA.Id,
            Zone = zoneA,
            TableNumber = "A-001",
            CoordX = 100.5,
            CoordY = 200.5,
            Capacity = 4,
            OperationalStatus = TableStatus.Reserved,
            ReservedByPublisherId = pubObeikan.Id,
            ReservedByPublisher = pubObeikan
        };

        var table2 = new MeetingTable
        {
            ZoneId = zoneA.Id,
            Zone = zoneA,
            TableNumber = "A-002",
            CoordX = 110.0,
            CoordY = 210.0,
            Capacity = 6,
            OperationalStatus = TableStatus.Reserved,
            ReservedByPublisherId = pubObeikan.Id,
            ReservedByPublisher = pubObeikan
        };

        var table3 = new MeetingTable
        {
            ZoneId = zoneB.Id,
            Zone = zoneB,
            TableNumber = "B-015",
            CoordX = 350.0,
            CoordY = 450.0,
            Capacity = 4,
            OperationalStatus = TableStatus.Reserved,
            ReservedByPublisherId = pubGallimard.Id,
            ReservedByPublisher = pubGallimard
        };

        context.MeetingTables.AddRange(table1, table2, table3);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPublisherReservedTablesAsync_WithEnglishTypo_ReturnsCorrectTables()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        await SeedSampleDataAsync(context);
        var service = new ConferenceAiToolsService(context);

        // Act - search with transposition typo "Obiekan" instead of "Obeikan"
        var results = await service.GetPublisherReservedTablesAsync("Obiekan");

        // Assert
        Assert.NotEmpty(results);
        var match = results[0];
        Assert.Equal("Obeikan Education", match.PublisherNameEn);
        Assert.Equal("SAU", match.CountryCode);
        Assert.Equal(2, match.ReservedTables.Count);
        Assert.Contains(match.ReservedTables, t => t.TableNumber == "A-001" && t.ZoneCode == "ZONE-A");
        Assert.Contains(match.ReservedTables, t => t.TableNumber == "A-002" && t.Capacity == 6);
    }

    [Fact]
    public async Task GetPublisherReservedTablesAsync_WithFrenchAccentOmitted_ReturnsGallimard()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        await SeedSampleDataAsync(context);
        var service = new ConferenceAiToolsService(context);

        // Act - search with missing accent and single 'l' typo "Galimard"
        var results = await service.GetPublisherReservedTablesAsync("Galimard");

        // Assert
        Assert.NotEmpty(results);
        var match = results[0];
        Assert.Equal("Éditions Gallimard", match.PublisherNameEn);
        Assert.Single(match.ReservedTables);
        Assert.Equal("B-015", match.ReservedTables[0].TableNumber);
        Assert.Equal("ZONE-B", match.ReservedTables[0].ZoneCode);
    }

    [Fact]
    public async Task GetPublisherReservedTablesAsync_WithArabicQuery_ReturnsCorrectTables()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        await SeedSampleDataAsync(context);
        var service = new ConferenceAiToolsService(context);

        // Act - search with Arabic partial keyword "العبيكان"
        var results = await service.GetPublisherReservedTablesAsync("العبيكان");

        // Assert
        Assert.NotEmpty(results);
        var match = results[0];
        Assert.Equal("مجموعة العبيكان للتعليم", match.PublisherNameAr);
        Assert.Equal(2, match.ReservedTables.Count);
    }

    [Fact]
    public async Task GetPublisherReservedTablesAsync_WithArabicTypoAndNoArticle_ReturnsDarAlTanweer()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        await SeedSampleDataAsync(context);
        var service = new ConferenceAiToolsService(context);

        // Act - search with "تنوير" (without 'دار' or 'ال')
        var results = await service.GetPublisherReservedTablesAsync("تنوير");

        // Assert
        Assert.NotEmpty(results);
        var match = results[0];
        Assert.Equal("Dar Al Tanweer", match.PublisherNameEn);
        Assert.Equal("دار التنوير", match.PublisherNameAr);
        Assert.Empty(match.ReservedTables); // Dar Al Tanweer has no reserved tables in seed
    }

    [Fact]
    public async Task GetPublisherReservedTablesAsync_WithEmptyQuery_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        await SeedSampleDataAsync(context);
        var service = new ConferenceAiToolsService(context);

        // Act
        var results = await service.GetPublisherReservedTablesAsync("   ");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetPublisherReservedTablesAsync_WithUnmatchedQuery_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        await SeedSampleDataAsync(context);
        var service = new ConferenceAiToolsService(context);

        // Act
        var results = await service.GetPublisherReservedTablesAsync("Nonexistent Completely Unknown Publisher XYZ");

        // Assert
        Assert.Empty(results);
    }
}
