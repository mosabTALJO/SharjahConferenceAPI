using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using Data;
using Data.Repositories;
using Data.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using OpenAI;
using Scalar.AspNetCore;
using SharjahConferenceAPI.Infrastructure;
using SharjahConferenceAPI.Services.Implementations;
using SharjahConferenceAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register DbContext and Data Access Layer
builder.Services.AddDbContext<EventDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Security & Authentication Services
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register Business Logic Services
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<IMeetingTableService, MeetingTableService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IConferenceAiToolsService, ConferenceAiToolsService>();
builder.Services.AddScoped<IChatService, ChatService>();

// Configure Google AI Studio (Gemini) with Microsoft.Extensions.AI
var geminiApiKey = builder.Configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "dummy-key";
var geminiModelId = builder.Configuration["Gemini:ModelId"] ?? "gemini-3.5-flash";
var geminiEndpoint = builder.Configuration["Gemini:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/openai/";

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var httpClient = new HttpClient(new GeminiThoughtSignatureHandler(new HttpClientHandler()));

    var openAiOptions = new OpenAIClientOptions
    {
        Endpoint = new Uri(geminiEndpoint),
        Transport = new HttpClientPipelineTransport(httpClient)
    };

    var openAiClient = new OpenAIClient(
        new ApiKeyCredential(geminiApiKey),
        openAiOptions);

    return openAiClient.GetChatClient(geminiModelId)
        .AsIChatClient()
        .AsBuilder()
        .UseFunctionInvocation()
        .Build(sp);
});

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "SharjahConferenceSecureSigningKey2026!#*SuperSecretLongKeyForJwtAuth";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SharjahConferenceAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SharjahConferenceClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://alsharjahpublishers.onrender.com" };

var cleanOrigins = corsOrigins
    .Select(origin => origin.TrimEnd('/'))
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(cleanOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Sharjah Conference API";
        document.Info.Version = "v1";
        document.Info.Description = "Sharjah Conference Management Platform REST API with JWT Authentication and Role-Based Authorization.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed initial default Admin user and conference dataset if needed
await DbInitializer.SeedDefaultAdminAsync(app.Services);
await DbInitializer.SeedConferenceDataAsync(app.Services);

app.Run();
