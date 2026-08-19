namespace Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Enums;
using Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharjahConferenceAPI.Services.Interfaces;

public static class DbInitializer
{
    public static async Task SeedDefaultAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        const string adminEmail = "admin@sharjahconference.ae";
        var existingAdmin = await unitOfWork.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                Email = adminEmail,
                PasswordHash = passwordHasher.HashPassword("Admin@123456"),
                Role = UserRole.Admin
            };

            await unitOfWork.Users.AddAsync(adminUser);
            await unitOfWork.CompleteAsync();
        }
    }

    /// <summary>
    /// Seeds comprehensive conference data: Venues, Zones, 72 Meeting Tables, 26 Bilingual Publishers,
    /// 56 Participants, corresponding User accounts with hashed credentials, interlocking meeting schedules,
    /// Facilities, and Announcements.
    /// </summary>
    public static async Task SeedConferenceDataAsync(IServiceProvider serviceProvider, bool forceRefresh = false)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // If forceRefresh is requested, clear existing conference data in correct foreign key order
        if (forceRefresh)
        {
            dbContext.Meetings.RemoveRange(dbContext.Meetings);
            dbContext.MeetingTables.RemoveRange(dbContext.MeetingTables);
            dbContext.Zones.RemoveRange(dbContext.Zones);
            dbContext.Venues.RemoveRange(dbContext.Venues);
            dbContext.Facilities.RemoveRange(dbContext.Facilities);
            dbContext.Announcements.RemoveRange(dbContext.Announcements);

            var nonAdminUsers = dbContext.Users.Where(u => u.Role != UserRole.Admin);
            dbContext.Users.RemoveRange(nonAdminUsers);
            dbContext.Publishers.RemoveRange(dbContext.Publishers);
            dbContext.Participants.RemoveRange(dbContext.Participants);

            await dbContext.SaveChangesAsync();
        }
        else if (await dbContext.Publishers.AnyAsync() || await dbContext.MeetingTables.AnyAsync())
            return;

        // ==========================================
        // 1. VENUES & ZONES
        // ==========================================
        var expoVenue = new Venue
        {
            NameEn = "Sharjah Expo Centre",
            NameAr = "مركز إكسبو الشارقة"
        };
        await dbContext.Venues.AddAsync(expoVenue);

        var zones = new List<Zone>
        {
            new()
            {
                VenueId = expoVenue.Id,
                Code = "ZONE-A",
                NameEn = "Zone A - Fiction & Literature",
                NameAr = "المنطقة أ - الأدب والرواية",
                SvgBounds = "{\"x\": 0, \"y\": 0, \"width\": 500, \"height\": 400, \"polygon\": \"0,0 500,0 500,400 0,400\"}"
            },
            new()
            {
                VenueId = expoVenue.Id,
                Code = "ZONE-B",
                NameEn = "Zone B - Academic & Scientific Publishing",
                NameAr = "المنطقة ب - النشر الأكاديمي والعلمي",
                SvgBounds = "{\"x\": 550, \"y\": 0, \"width\": 500, \"height\": 400, \"polygon\": \"550,0 1050,0 1050,400 550,400\"}"
            },
            new()
            {
                VenueId = expoVenue.Id,
                Code = "ZONE-C",
                NameEn = "Zone C - Children's & YA Books",
                NameAr = "المنطقة ج - كتب الأطفال واليافعين",
                SvgBounds = "{\"x\": 1100, \"y\": 0, \"width\": 500, \"height\": 400, \"polygon\": \"1100,0 1600,0 1600,400 1100,400\"}"
            },
            new()
            {
                VenueId = expoVenue.Id,
                Code = "ZONE-D",
                NameEn = "Zone D - Digital & Rights Exchange",
                NameAr = "المنطقة د - التبادل الرقمي والحقوق",
                SvgBounds = "{\"x\": 0, \"y\": 450, \"width\": 500, \"height\": 400, \"polygon\": \"0,450 500,450 500,850 0,850\"}"
            },
            new()
            {
                VenueId = expoVenue.Id,
                Code = "ZONE-E",
                NameEn = "Zone E - International Publishers",
                NameAr = "المنطقة هـ - الناشرون الدوليون",
                SvgBounds = "{\"x\": 550, \"y\": 450, \"width\": 500, \"height\": 400, \"polygon\": \"550,450 1050,450 1050,850 550,850\"}"
            },
            new()
            {
                VenueId = expoVenue.Id,
                Code = "ZONE-F",
                NameEn = "Zone F - Translation & Illustrated Books",
                NameAr = "المنطقة و - الترجمة والكتب المصورة",
                SvgBounds = "{\"x\": 1100, \"y\": 450, \"width\": 500, \"height\": 400, \"polygon\": \"1100,450 1600,450 1600,850 1100,850\"}"
            }
        };
        await dbContext.Zones.AddRangeAsync(zones);

        // ==========================================
        // 2. PUBLISHERS & PUBLISHER USERS (26 Publishers)
        // ==========================================
        var publisherDefinitions = new (string NameEn, string NameAr, string CountryCode, string Email)[]
        {
            ("Kalimat Group", "مجموعة كلمات", "ARE", "contact@kalimatgroup.ae"),
            ("Dar Al Shorouk", "دار الشروق", "EGY", "rights@shorouk.com"),
            ("Jarir Publications", "منشورات جرير", "SAU", "publishing@jarir.com"),
            ("Dar Al Saqi", "دار الساقي", "LBN", "info@daralsaqi.com"),
            ("Bloomsbury Publishing", "دار بلومزبري للنشر", "GBR", "rights@bloomsbury.com"),
            ("Penguin Random House", "بنغوين راندوم هاوس", "USA", "internationalrights@penguinrandomhouse.com"),
            ("Hachette Livre", "أشيت ليفتر", "FRA", "foreignrights@hachette.com"),
            ("Dar Al Hilal", "دار الهلال", "EGY", "info@daralhilal.com"),
            ("Arab Scientific Publishers", "الدار العربية للعلوم ناشرون", "LBN", "rights@asp.com.lb"),
            ("Al Kotob Khan", "الكتب خان", "EGY", "editorial@kotobkhan.com"),
            ("Medina Publishing", "دار المدينة للنشر", "GBR", "rights@medinapublishing.com"),
            ("Dar Al-Adab", "دار الآداب", "LBN", "info@daraladab.com"),
            ("Obeikan Education", "مجموعة العبيكان للتعليم", "SAU", "licensing@obeikan.com.sa"),
            ("Dar Al Tanweer", "دار التنوير", "LBN", "rights@daraltanweer.com"),
            ("Maktabat Al Falah", "مكتبة الفلاح", "KWT", "info@maktabatalfalah.com"),
            ("Dar Al Mada", "دار المدى", "IRQ", "contact@daralmada.com"),
            ("Éditions Gallimard", "منشورات غاليمار", "FRA", "droits-etrangers@gallimard.fr"),
            ("Suhrkamp Verlag", "دار زوركامب للنشر", "DEU", "foreignrights@suhrkamp.de"),
            ("Al Mutawassit Publications", "منشورات المتوسط", "ITA", "info@almutawassit.it"),
            ("Takween Publishing", "منشورات تكوين", "KWT", "rights@takween.me"),
            ("Dar Al Rawafed", "دار الروافد الثقافية", "LBN", "info@daralrawafed.com"),
            ("Dar Al Mufradat", "دار المفردات", "SAU", "publishing@almufradat.com"),
            ("Grupo Planeta", "مجموعة بلانيتا للنشر", "ESP", "derechos@planeta.es"),
            ("Dar Athar", "دار أثر للنشر والتوزيع", "SAU", "info@darathar.com"),
            ("Emirates Publishers Association Press", "منشورات جمعية الناشرين الإماراتيين", "ARE", "press@epa.org.ae"),
            ("Can Yayınları", "منشورات جان", "TUR", "rights@canyayinlari.com")
        };

        var publisherPasswordHash = passwordHasher.HashPassword("Publisher@123");
        var publishers = new List<Publisher>();
        var publisherUsers = new List<User>();

        foreach (var def in publisherDefinitions)
        {
            var pub = new Publisher
            {
                NameEn = def.NameEn,
                NameAr = def.NameAr,
                CountryCode = def.CountryCode
            };
            publishers.Add(pub);

            var user = new User
            {
                Email = def.Email.ToLowerInvariant(),
                PasswordHash = publisherPasswordHash,
                Role = UserRole.Publisher,
                PublisherId = pub.Id,
                Publisher = pub
            };
            publisherUsers.Add(user);
        }

        await dbContext.Publishers.AddRangeAsync(publishers);
        await dbContext.Users.AddRangeAsync(publisherUsers);

        // ==========================================
        // 3. PARTICIPANTS & PARTICIPANT USERS (56 Participants)
        // ==========================================
        var participantDefinitions = new (string FirstName, string LastName, string Email, string Phone, string Title)[]
        {
            ("Sarah", "Jenkins", "sarah.jenkins@londonlitagency.co.uk", "+44 20 7946 0912", "Senior Literary Agent"),
            ("Tariq", "Mansoor", "tariq.mansoor@gulfrights.ae", "+971 50 123 4567", "Director of Acquisitions & Rights"),
            ("Elena", "Rostova", "elena.rostova@slavictranslations.org", "+49 30 555 0192", "Lead Translation Scout"),
            ("Omar", "Al-Hashimi", "omar.hashimi@cairolit.com", "+20 10 2345 6789", "Managing Editor & Rights Lead"),
            ("Claire", "Dubois", "claire.dubois@parisrights.fr", "+33 1 42 68 55 00", "International Licensing Director"),
            ("Fahad", "Al-Otaibi", "fahad.otaibi@riyadhmedia.sa", "+966 55 987 6543", "Head of Digital Content & Audio"),
            ("Amina", "Nasser", "amina.nasser@beirutpress.org", "+961 1 345 678", "Children's & YA Book Editor"),
            ("Marcus", "Vance", "marcus.vance@nycliterary.com", "+1 212 555 0184", "Executive Vice President, Global Rights"),
            ("Layla", "Kassab", "layla.kassab@ammanagency.jo", "+962 6 567 8901", "Arabic Fiction Scout & Consultant"),
            ("Kenji", "Takahashi", "kenji.takahashi@tokyobooks.jp", "+81 3 5555 0143", "Senior Manga & Graphic Novel Rights Manager"),
            ("Fatima", "Al-Zahra", "fatima.zahra@sharjahculture.gov.ae", "+971 55 432 1098", "Conference Delegate & Literary Fellow"),
            ("Matteo", "Rossi", "matteo.rossi@milanobooks.it", "+39 02 555 0177", "Academic & Non-Fiction Licensing Officer"),
            ("Noura", "Al-Qasimi", "noura.qasimi@emiratesheritage.ae", "+971 50 876 5432", "Heritage & Illustrated Books Coordinator"),
            ("David", "Kowalski", "david.kowalski@warsawrights.pl", "+48 22 555 0165", "Central Europe Co-Publishing Specialist"),
            ("Zainab", "Bouzid", "zainab.bouzid@casabooks.ma", "+212 5 22 33 44 55", "Maghreb Rights & Translation Scout"),
            ("Alexander", "Weber", "alexander.weber@berlinagency.de", "+49 30 890 1234", "Head of Scientific & Academic Content"),
            ("Hassan", "Al-Bahrani", "hassan.bahrani@manamamedia.bh", "+973 39 123 456", "Regional Distribution Specialist"),
            ("Sophie", "Martin", "sophie.martin@brusselspublishing.be", "+32 2 555 0122", "Illustrated & Children's Rights Director"),
            ("Khaled", "Al-Ghamdi", "khaled.ghamdi@jeddahrights.sa", "+966 50 345 6789", "Acquisitions Consultant"),
            ("Isabella", "Santos", "isabella.santos@madridlit.es", "+34 91 555 0134", "Ibero-American Translation Director"),
            ("Youssef", "El-Masry", "youssef.masry@alexandriabooks.eg", "+20 12 8765 4321", "Arabic Literary Agent"),
            ("Anna", "Larsson", "anna.larsson@nordicrights.se", "+46 8 555 0188", "Scandinavian Rights Scout"),
            ("Bader", "Al-Sabah", "bader.sabah@kuwaitculture.org", "+965 99 876 543", "Senior Publishing Consultant"),
            ("Charlotte", "Bennett", "charlotte.bennett@oxfordcontent.co.uk", "+44 1865 555 019", "Educational Content Licensing Specialist"),
            ("Mustafa", "Demir", "mustafa.demir@istanbulrights.tr", "+90 212 555 0156", "Bilingual Rights Negotiator"),
            ("Hessa", "Al-Nuaimi", "hessa.nuaimi@dubaiarts.ae", "+971 52 345 6789", "Digital Formats & Audio Rights Specialist"),
            ("Lucas", "Moreau", "lucas.moreau@genevapress.ch", "+41 22 555 0144", "International Contract Legal Counsel"),
            ("Rania", "Salameh", "rania.salameh@ramallahculture.ps", "+970 59 123 4567", "Poetry & Translation Curator"),
            ("Julian", "Schumacher", "julian.schumacher@viennesebooks.at", "+43 1 555 0178", "Rights & Licensing Officer"),
            ("Mona", "Al-Kuwari", "mona.kuwari@dohaliterary.qa", "+974 55 123 456", "Senior Acquisitions Specialist"),
            ("Robert", "Chen", "robert.chen@singaporerights.sg", "+65 6789 0123", "Asia-Pacific Rights Director"),
            ("Reem", "Al-Saeed", "reem.saeed@sharjahrights.ae", "+971 56 789 0123", "Fiction & YA Content Scout"),
            ("Gabriel", "Silva", "gabriel.silva@saopaulolit.br", "+55 11 5555 0199", "Latin American Rights Negotiator"),
            ("Dina", "Haddad", "dina.haddad@damascuseditorial.sy", "+963 93 123 4567", "Senior Translation Editor"),
            ("Oliver", "Smith", "oliver.smith@sydneybooks.au", "+61 2 5555 0123", "Oceania Rights & Licensing Scout"),
            ("Asma", "Al-Ali", "asma.ali@adbookfair.ae", "+971 50 654 3210", "Program Coordinator & Rights Fellow"),
            ("Patrick", "O'Connor", "patrick.oconnor@dublinlit.ie", "+353 1 555 0167", "Irish Literature Rights Representative"),
            ("Samira", "Benali", "samira.benali@algiersagency.dz", "+213 55 123 4567", "North African Publishing Consultant"),
            ("Jonas", "Lindqvist", "jonas.lindqvist@helsinkirights.fi", "+358 9 555 0145", "Nordic Translation Project Lead"),
            ("Mariam", "Al-Balushi", "mariam.balushi@muscatmedia.om", "+968 91 234 567", "Gulf Rights & Distribution Officer"),
            ("Antoine", "Mercier", "antoine.mercier@montrealpress.ca", "+1 514 555 0182", "Francophone Rights Manager"),
            ("Hala", "Al-Sharif", "hala.sharif@ammanpublishing.jo", "+962 7 9123 4567", "Arabic Children's Books Author & Agent"),
            ("Klaus", "Muller", "klaus.muller@munichrights.de", "+49 89 555 0198", "Illustrated Non-Fiction Editor"),
            ("Salma", "Al-Kaabi", "salma.kaabi@ajmanpress.ae", "+971 54 321 0987", "Audiobook Acquisitions Producer"),
            ("Liam", "Davies", "liam.davies@cardiffbooks.co.uk", "+44 29 2055 0111", "UK Regional Rights Scout"),
            ("Zuhair", "Al-Khatib", "zuhair.khatib@baghdadlit.iq", "+964 77 1234 5678", "Historical Fiction & Academic Scout"),
            ("Camilla", "Bertolini", "camilla.bertolini@romarights.it", "+39 06 555 0132", "Italian Fiction & Non-Fiction Scout"),
            ("Hamad", "Al-Majid", "hamad.majid@khobarbooks.sa", "+966 54 123 4567", "Licensing & Co-Edition Manager"),
            ("Emily", "Watson", "emily.watson@bostonagency.com", "+1 617 555 0176", "Young Adult & Graphic Novels Agent"),
            ("Adel", "Soliman", "adel.soliman@tantapress.eg", "+20 11 9876 5432", "Senior Translation Consultant"),
            ("Helena", "Vogel", "helena.vogel@zurichrights.ch", "+41 44 555 0129", "European Co-Publishing Lead"),
            ("Ibrahim", "Al-Dossary", "ibrahim.dossary@dammamagency.sa", "+966 56 789 0123", "Children's Digital Content Specialist"),
            ("Victoria", "Gray", "victoria.gray@edinburghbooks.co.uk", "+44 131 555 0155", "Fiction Commissioning Scout"),
            ("Marwan", "Al-Taher", "marwan.taher@tunisrights.tn", "+216 20 123 456", "Francophone-Arab Translation Specialist"),
            ("Chloe", "Martin", "chloe.martin@lyonpublishing.fr", "+33 4 72 55 0190", "Comics & Graphic Novel Rights Agent"),
            ("Nasser", "Al-Falasi", "nasser.falasi@uaeliterature.ae", "+971 55 987 1234", "Executive Director, Rights Exchange")
        };

        var participantPasswordHash = passwordHasher.HashPassword("Participant@123");
        var participants = new List<Participant>();
        var participantUsers = new List<User>();

        foreach (var def in participantDefinitions)
        {
            var part = new Participant
            {
                FirstName = def.FirstName,
                LastName = def.LastName,
                Email = def.Email.ToLowerInvariant(),
                Phone = def.Phone,
                Title = def.Title
            };
            participants.Add(part);

            var user = new User
            {
                Email = def.Email.ToLowerInvariant(),
                PasswordHash = participantPasswordHash,
                Role = UserRole.Participant,
                ParticipantId = part.Id,
                Participant = part
            };
            participantUsers.Add(user);
        }

        await dbContext.Participants.AddRangeAsync(participants);
        await dbContext.Users.AddRangeAsync(participantUsers);

        // ==========================================
        // 4. MEETING TABLES (72 Tables Across 6 Zones)
        // ==========================================
        // 12 tables per zone (T-01 to T-72).
        // First 26 tables reserved for the 26 publishers.
        var tables = new List<MeetingTable>();
        int tableCounter = 1;

        for (int zIndex = 0; zIndex < zones.Count; zIndex++)
        {
            var zone = zones[zIndex];
            // Base coordinates for the zone layout
            double originX = (zIndex % 3) * 550.0;
            double originY = (zIndex / 3) * 450.0;

            // 12 tables arranged in 3 rows x 4 columns within each zone
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    double coordX = originX + 70.0 + (col * 100.0);
                    double coordY = originY + 80.0 + (row * 90.0);
                    int capacity = (tableCounter % 3 == 0) ? 6 : (tableCounter % 3 == 1) ? 4 : 8;

                    Guid? reservedPublisherId = null;
                    var status = TableStatus.Available;

                    // Reserve the first 26 tables for publishers
                    if (tableCounter <= publishers.Count)
                    {
                        var publisherForTable = publishers[tableCounter - 1];
                        reservedPublisherId = publisherForTable.Id;
                        status = TableStatus.Reserved;
                    }

                    var table = new MeetingTable
                    {
                        ZoneId = zone.Id,
                        TableNumber = $"T-{tableCounter:D2}",
                        CoordX = Math.Round(coordX, 1),
                        CoordY = Math.Round(coordY, 1),
                        Capacity = capacity,
                        OperationalStatus = status,
                        ReservedByPublisherId = reservedPublisherId
                    };

                    tables.Add(table);
                    tableCounter++;
                }
            }
        }

        await dbContext.MeetingTables.AddRangeAsync(tables);

        // ==========================================
        // 5. INTERLOCKING MEETING SCHEDULES (70+ Meetings)
        // ==========================================
        // Conference Dates: Day 1 (2026-11-03), Day 2 (2026-11-04), Day 3 (2026-11-05)
        // 8 Daily 45-minute Meeting Slots:
        // Slot 0: 09:00 - 09:45
        // Slot 1: 10:00 - 10:45
        // Slot 2: 11:00 - 11:45
        // Slot 3: 12:00 - 12:45
        // Slot 4: 14:00 - 14:45
        // Slot 5: 15:00 - 15:45
        // Slot 6: 16:00 - 16:45
        // Slot 7: 17:00 - 17:45
        var meetingSlotTimes = new (int StartHour, int StartMin, int EndHour, int EndMin)[]
        {
            (9, 0, 9, 45),
            (10, 0, 10, 45),
            (11, 0, 11, 45),
            (12, 0, 12, 45),
            (14, 0, 14, 45),
            (15, 0, 15, 45),
            (16, 0, 16, 45),
            (17, 0, 17, 45)
        };

        var negotiationTopics = new string[]
        {
            "Arabic translation rights negotiation for bestselling fiction titles.",
            "Digital audiobook and regional streaming distribution rights.",
            "Co-publishing agreement for illustrated children's encyclopedias.",
            "Territorial licensing for academic and scientific university press books.",
            "Graphic novel translation rights and character licensing discussion.",
            "Exploration of Middle East distribution partnerships and advance royalties.",
            "Rights scouting for European award-winning young adult novels.",
            "Discussion on translation grants and governmental cultural subsidy programs.",
            "Licensing agreement for digital eBook library lending across public schools.",
            "Final contract terms and copyright indemnity clauses review."
        };

        var meetings = new List<Meeting>();
        int totalParticipants = participants.Count; // 56
        int reservedPublisherCount = publishers.Count; // 26

        // Day 1: Slots 0, 1, 2, 4, 5 (Completed and InProgress sessions)
        var day1 = new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc);
        int[] day1Slots = [0, 1, 2, 4, 5];

        foreach (int slotIdx in day1Slots)
        {
            var slot = meetingSlotTimes[slotIdx];
            var startTime = day1.AddHours(slot.StartHour).AddMinutes(slot.StartMin);
            var endTime = day1.AddHours(slot.EndHour).AddMinutes(slot.EndMin);

            for (int pubIdx = 0; pubIdx < reservedPublisherCount; pubIdx++)
            {
                var pub = publishers[pubIdx];
                var table = tables[pubIdx]; // Reserved table corresponding to publisher

                // Interlocking mathematical permutation: gcd(3, 56) = 1 guarantees zero participant conflicts
                int participantIdx = (pubIdx * 3 + slotIdx) % totalParticipants;
                var part = participants[participantIdx];

                var status = slotIdx <= 2 ? MeetingStatus.Completed :
                             slotIdx == 4 ? MeetingStatus.Completed : MeetingStatus.InProgress;

                var topic = negotiationTopics[(pubIdx + slotIdx) % negotiationTopics.Length];

                meetings.Add(new Meeting
                {
                    MeetingTableId = table.Id,
                    PublisherId = pub.Id,
                    ParticipantId = part.Id,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = status,
                    Notes = $"{topic} - Scheduled with {part.FirstName} {part.LastName} ({part.Title}) at {table.TableNumber}."
                });
            }
        }

        // Day 2: Slots 0, 1, 3, 5, 6 (Scheduled meetings)
        var day2 = new DateTime(2026, 11, 4, 0, 0, 0, DateTimeKind.Utc);
        int[] day2Slots = [0, 1, 3, 5, 6];

        foreach (int slotIdx in day2Slots)
        {
            var slot = meetingSlotTimes[slotIdx];
            var startTime = day2.AddHours(slot.StartHour).AddMinutes(slot.StartMin);
            var endTime = day2.AddHours(slot.EndHour).AddMinutes(slot.EndMin);

            for (int pubIdx = 0; pubIdx < reservedPublisherCount; pubIdx++)
            {
                var pub = publishers[pubIdx];
                var table = tables[pubIdx];

                int participantIdx = (pubIdx * 3 + slotIdx + 11) % totalParticipants;
                var part = participants[participantIdx];

                var topic = negotiationTopics[(pubIdx + slotIdx + 3) % negotiationTopics.Length];

                meetings.Add(new Meeting
                {
                    MeetingTableId = table.Id,
                    PublisherId = pub.Id,
                    ParticipantId = part.Id,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = MeetingStatus.Scheduled,
                    Notes = $"{topic} - Scheduled with {part.FirstName} {part.LastName} ({part.Title}) at {table.TableNumber}."
                });
            }
        }

        // Day 3: Slots 1, 2, 4, 6 (Scheduled, some Cancelled / NoShow for testing)
        var day3 = new DateTime(2026, 11, 5, 0, 0, 0, DateTimeKind.Utc);
        int[] day3Slots = [1, 2, 4, 6];

        foreach (int slotIdx in day3Slots)
        {
            var slot = meetingSlotTimes[slotIdx];
            var startTime = day3.AddHours(slot.StartHour).AddMinutes(slot.StartMin);
            var endTime = day3.AddHours(slot.EndHour).AddMinutes(slot.EndMin);

            // Schedule for 20 publishers on day 3
            for (int pubIdx = 0; pubIdx < 20; pubIdx++)
            {
                var pub = publishers[pubIdx];
                var table = tables[pubIdx];

                int participantIdx = (pubIdx * 3 + slotIdx + 23) % totalParticipants;
                var part = participants[participantIdx];

                var status = MeetingStatus.Scheduled;
                if (slotIdx == 2 && (pubIdx == 3 || pubIdx == 7))
                {
                    status = MeetingStatus.Cancelled;
                }
                else if (slotIdx == 4 && pubIdx == 5)
                {
                    status = MeetingStatus.NoShow;
                }

                var topic = negotiationTopics[(pubIdx + slotIdx + 7) % negotiationTopics.Length];

                meetings.Add(new Meeting
                {
                    MeetingTableId = table.Id,
                    PublisherId = pub.Id,
                    ParticipantId = part.Id,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = status,
                    Notes = $"{topic} - Scheduled with {part.FirstName} {part.LastName} ({part.Title}) at {table.TableNumber}."
                });
            }
        }

        await dbContext.Meetings.AddRangeAsync(meetings);

        // ==========================================
        // 6. FACILITIES (Points of Interest)
        // ==========================================
        var facilities = new List<Facility>
        {
            new()
            {
                Name = "Main Registration & Badge Collection Desk",
                Category = "Registration",
                CoordX = 120.0,
                CoordY = 40.0
            },
            new()
            {
                Name = "VIP & Rights Buyers Executive Lounge",
                Category = "Lounge",
                CoordX = 620.0,
                CoordY = 40.0
            },
            new()
            {
                Name = "Press & International Media Centre",
                Category = "Media",
                CoordX = 1180.0,
                CoordY = 40.0
            },
            new()
            {
                Name = "Men's Prayer Room (Musalla)",
                Category = "Prayer",
                CoordX = 160.0,
                CoordY = 880.0
            },
            new()
            {
                Name = "Women's Prayer Room (Musalla)",
                Category = "Prayer",
                CoordX = 360.0,
                CoordY = 880.0
            },
            new()
            {
                Name = "Al Majlis Central Networking Cafe & Dining",
                Category = "Food & Beverage",
                CoordX = 800.0,
                CoordY = 420.0
            },
            new()
            {
                Name = "Espresso & Pastry Kiosk A (Zone A)",
                Category = "Food & Beverage",
                CoordX = 260.0,
                CoordY = 220.0
            },
            new()
            {
                Name = "Espresso & Pastry Kiosk B (Zone C)",
                Category = "Food & Beverage",
                CoordX = 1360.0,
                CoordY = 220.0
            },
            new()
            {
                Name = "Simultaneous Translation Services & Audio Equipment Hub",
                Category = "Services",
                CoordX = 1220.0,
                CoordY = 880.0
            },
            new()
            {
                Name = "IT Support & Digital Concierge Desk",
                Category = "Support",
                CoordX = 660.0,
                CoordY = 880.0
            }
        };

        await dbContext.Facilities.AddRangeAsync(facilities);

        // ==========================================
        // 7. ANNOUNCEMENTS
        // ==========================================
        var now = DateTime.UtcNow;
        var announcements = new List<Announcement>
        {
            new()
            {
                Title = "Welcome to Sharjah International Publishers Conference 2026",
                Body = "We are delighted to welcome all publishers, literary agents, and translation fellows. Registration desks are open in Hall 1. Please ensure your digital badge is visible at all meeting tables.",
                Priority = AnnouncementPriority.Urgent,
                ActiveStartTime = now.AddDays(-1),
                ActiveEndTime = now.AddDays(7)
            },
            new()
            {
                Title = "Sharjah Translation Rights Grant Applications Now Open",
                Body = "Publishers negotiating Arabic translation rights can apply for the Sharjah Translation Grant ($4,000 for general titles, $1,500 for children's books) at the Rights Centre Desk.",
                Priority = AnnouncementPriority.High,
                ActiveStartTime = now.AddDays(-1),
                ActiveEndTime = now.AddDays(7)
            },
            new()
            {
                Title = "Networking Lunch in Al Majlis Hall",
                Body = "All registered delegates and badge holders are invited to the daily networking luncheon served in Al Majlis Central Hall from 13:00 to 14:00.",
                Priority = AnnouncementPriority.Normal,
                ActiveStartTime = now.AddDays(-1),
                ActiveEndTime = now.AddDays(7)
            },
            new()
            {
                Title = "Panel Session: AI in Global Publishing & Licensing",
                Body = "Join industry leaders today at 16:30 in Auditorium B for a discussion on generative AI copyright frameworks and rights management tools.",
                Priority = AnnouncementPriority.High,
                ActiveStartTime = now.AddDays(-1),
                ActiveEndTime = now.AddDays(7)
            },
            new()
            {
                Title = "Complimentary Shuttle Service to Partner Hotels",
                Body = "Evening shuttles will depart every 20 minutes from the main gate to Sharjah and Dubai partner hotels starting from 18:00 until 21:00.",
                Priority = AnnouncementPriority.Normal,
                ActiveStartTime = now.AddDays(-1),
                ActiveEndTime = now.AddDays(7)
            }
        };

        await dbContext.Announcements.AddRangeAsync(announcements);

        // Save all seeded entities
        await dbContext.SaveChangesAsync();
    }
}

