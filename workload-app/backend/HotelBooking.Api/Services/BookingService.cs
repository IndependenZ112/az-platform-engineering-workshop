using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class BookingService(HotelDbContext db)
{
    public async Task<BookingConfirmation?> CreateAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        var hotel = await db.Hotels.Include(h => h.Rooms).FirstOrDefaultAsync(h => h.Id == request.HotelId, ct);
        if (hotel == null) return null;

        var room = hotel.Rooms.FirstOrDefault(r => r.Id == request.RoomId && r.IsAvailable);
        if (room == null) return null;

        var nights = (request.CheckOut - request.CheckIn).Days;
        if (nights <= 0) return null;

        var booking = new Booking(
            Id: $"BK-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            HotelId: request.HotelId,
            RoomId: request.RoomId,
            GuestName: request.GuestName,
            GuestEmail: request.GuestEmail,
            CheckIn: request.CheckIn,
            CheckOut: request.CheckOut,
            NumberOfGuests: request.NumberOfGuests,
            TotalPrice: room.PricePerNight * nights,
            Status: BookingStatus.Confirmed,
            CreatedAt: DateTime.UtcNow
        );

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        return new BookingConfirmation(
            booking.Id, hotel.Name, room.Type,
            booking.CheckIn, booking.CheckOut,
            booking.TotalPrice, booking.Status
        );
    }

    public Task<Booking?> GetByIdAsync(string id, CancellationToken ct = default) =>
        db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<List<Booking>> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Bookings.Where(b => b.GuestEmail == email).ToListAsync(ct);

    public async Task<bool> CancelAsync(string id, CancellationToken ct = default)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.Status == BookingStatus.Confirmed, ct);
        if (booking == null) return false;

        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public Task<List<Booking>> GetAllAsync(CancellationToken ct = default) => db.Bookings.ToListAsync(ct);
}
