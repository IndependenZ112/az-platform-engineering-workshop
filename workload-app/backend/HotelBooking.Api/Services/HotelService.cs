using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class HotelService(HotelDbContext db)
{
    public async Task<List<HotelSummary>> SearchAsync(HotelSearchRequest request, CancellationToken ct = default)
    {
        var query = db.Hotels.Include(h => h.Rooms).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(h => EF.Functions.Like(h.City, $"%{request.City}%"));

        if (!string.IsNullOrWhiteSpace(request.Country))
            query = query.Where(h => EF.Functions.Like(h.Country, $"%{request.Country}%"));

        if (request.MinStars.HasValue)
            query = query.Where(h => h.StarRating >= request.MinStars.Value);

        if (request.Guests.HasValue)
            query = query.Where(h => h.Rooms.Any(r => r.MaxGuests >= request.Guests.Value && r.IsAvailable));

        if (request.MaxPrice.HasValue)
            query = query.Where(h => h.Rooms.Any(r => r.PricePerNight <= request.MaxPrice.Value && r.IsAvailable));

        var hotels = await query.ToListAsync(ct);

        return hotels.Select(h => new HotelSummary(
            h.Id, h.Name, h.City, h.Country, h.StarRating, h.ImageUrl,
            h.Rooms.Where(r => r.IsAvailable).Min(r => r.PricePerNight),
            h.Amenities
        )).ToList();
    }

    public Task<Hotel?> GetByIdAsync(string id, CancellationToken ct = default) =>
        db.Hotels.Include(h => h.Rooms).FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<List<Room>> GetAvailableRoomsAsync(string hotelId, int? guests = null, CancellationToken ct = default)
    {
        var query = db.Rooms.Where(r => r.HotelId == hotelId && r.IsAvailable);
        if (guests.HasValue)
            query = query.Where(r => r.MaxGuests >= guests.Value);
        return await query.ToListAsync(ct);
    }
}
