using Azure.Monitor.OpenTelemetry.AspNetCore;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database (SQL Server). Connection string "HotelDb" — for the workshop this points at
// LocalDB locally and at the private-endpoint Azure SQL with Entra/managed identity auth
// once Chore 2 is complete (e.g. Authentication=Active Directory Default).
var hotelDbConnectionString = builder.Configuration.GetConnectionString("HotelDb")
    ?? throw new InvalidOperationException("ConnectionStrings:HotelDb is not configured.");
builder.Services.AddDbContext<HotelDbContext>(opts => opts.UseSqlServer(hotelDbConnectionString));

// Services (scoped — each request gets its own DbContext)
builder.Services.AddScoped<HotelService>();
builder.Services.AddScoped<BookingService>();

// OpenTelemetry → Azure Monitor. No-op unless APPLICATIONINSIGHTS_CONNECTION_STRING is set
// (the platform team injects it from the App Insights resource in the workload spoke).
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// OpenAPI
builder.Services.AddOpenApi();

// CORS (allow frontend dev server)
builder.Services.AddCors(o => o.AddDefaultPolicy(b =>
    b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Ensure the database exists and seed if empty
await DbInitializer.InitializeAsync(app.Services, app.Logger);

app.UseCors();
app.MapOpenApi();

// Serve the React frontend as static files (production)
app.UseDefaultFiles();
app.UseStaticFiles();

// ──────────────────────────────────────────────
//  REST API: Hotels
// ──────────────────────────────────────────────
var hotelsApi = app.MapGroup("/api/hotels").WithTags("Hotels");

hotelsApi.MapGet("/", async (HotelService svc, string? city, string? country, int? minStars, decimal? maxPrice, int? guests, CancellationToken ct) =>
    await svc.SearchAsync(new HotelSearchRequest(city, country, null, null, guests, minStars, maxPrice), ct))
    .WithName("SearchHotels")
    .WithDescription("Search hotels with optional filters");

hotelsApi.MapGet("/{id}", async (HotelService svc, string id, CancellationToken ct) =>
    await svc.GetByIdAsync(id, ct) is { } hotel ? Results.Ok(hotel) : Results.NotFound())
    .WithName("GetHotel")
    .WithDescription("Get hotel details by ID");

hotelsApi.MapGet("/{id}/rooms", async (HotelService svc, string id, int? guests, CancellationToken ct) =>
    await svc.GetAvailableRoomsAsync(id, guests, ct))
    .WithName("GetAvailableRooms")
    .WithDescription("Get available rooms for a hotel");

// ──────────────────────────────────────────────
//  REST API: Bookings
// ──────────────────────────────────────────────
var bookingsApi = app.MapGroup("/api/bookings").WithTags("Bookings");

bookingsApi.MapPost("/", async (BookingService svc, CreateBookingRequest req, CancellationToken ct) =>
    await svc.CreateAsync(req, ct) is { } confirmation ? Results.Created($"/api/bookings/{confirmation.BookingId}", confirmation) : Results.BadRequest("Invalid booking request"))
    .WithName("CreateBooking")
    .WithDescription("Create a new booking");

bookingsApi.MapGet("/{id}", async (BookingService svc, string id, CancellationToken ct) =>
    await svc.GetByIdAsync(id, ct) is { } booking ? Results.Ok(booking) : Results.NotFound())
    .WithName("GetBooking")
    .WithDescription("Get booking by ID");

bookingsApi.MapGet("/by-email/{email}", async (BookingService svc, string email, CancellationToken ct) =>
    await svc.GetByEmailAsync(email, ct))
    .WithName("GetBookingsByEmail")
    .WithDescription("List bookings for a guest email");

bookingsApi.MapDelete("/{id}", async (BookingService svc, string id, CancellationToken ct) =>
    await svc.CancelAsync(id, ct) ? Results.Ok("Cancelled") : Results.NotFound())
    .WithName("CancelBooking")
    .WithDescription("Cancel a booking");

// SPA fallback — serve index.html for any unmatched routes (must be after all API routes)
app.MapFallbackToFile("index.html");

app.Run();
