namespace HotelBooking.Api.Models;

// Booking is a mutable entity (Status changes on cancellation), so it's a class
// rather than a record. The named-argument call sites stay the same.
public class Booking(
    string Id,
    string HotelId,
    string RoomId,
    string GuestName,
    string GuestEmail,
    DateTime CheckIn,
    DateTime CheckOut,
    int NumberOfGuests,
    decimal TotalPrice,
    BookingStatus Status,
    DateTime CreatedAt)
{
    public string Id { get; init; } = Id;
    public string HotelId { get; init; } = HotelId;
    public string RoomId { get; init; } = RoomId;
    public string GuestName { get; init; } = GuestName;
    public string GuestEmail { get; init; } = GuestEmail;
    public DateTime CheckIn { get; init; } = CheckIn;
    public DateTime CheckOut { get; init; } = CheckOut;
    public int NumberOfGuests { get; init; } = NumberOfGuests;
    public decimal TotalPrice { get; init; } = TotalPrice;
    public BookingStatus Status { get; set; } = Status;
    public DateTime CreatedAt { get; init; } = CreatedAt;
}

public enum BookingStatus
{
    Confirmed,
    Cancelled,
    Completed
}

public record CreateBookingRequest(
    string HotelId,
    string RoomId,
    string GuestName,
    string GuestEmail,
    DateTime CheckIn,
    DateTime CheckOut,
    int NumberOfGuests
);

public record BookingConfirmation(
    string BookingId,
    string HotelName,
    string RoomType,
    DateTime CheckIn,
    DateTime CheckOut,
    decimal TotalPrice,
    BookingStatus Status
);
