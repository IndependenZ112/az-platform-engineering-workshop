using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Data;

public static class DbInitializer
{
    /// <summary>
    /// Ensures the database exists and seeds the hotel catalogue if it's empty.
    /// Safe to run on every startup — does nothing when data is already present.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Hotels.AnyAsync(ct))
        {
            logger.LogInformation("Hotel data already present — skipping seed.");
            return;
        }

        var hotels = SeedData.GetHotels();
        db.Hotels.AddRange(hotels);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} hotels into the database.", hotels.Count);
    }
}
