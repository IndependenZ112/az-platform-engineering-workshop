namespace HotelBooking.Api.Models;

public record Hotel(
    string Id,
    string Name,
    string Description,
    string City,
    string Country,
    string Address,
    double Latitude,
    double Longitude,
    int StarRating,
    string ImageUrl,
    List<string> Amenities,
    List<Room> Rooms
);

public record Room(
    string Id,
    string HotelId,
    string Type,
    string Description,
    int MaxGuests,
    decimal PricePerNight,
    bool IsAvailable
);

public record HotelSearchRequest(
    string? City = null,
    string? Country = null,
    DateTime? CheckIn = null,
    DateTime? CheckOut = null,
    int? Guests = null,
    int? MinStars = null,
    decimal? MaxPrice = null
);

public record HotelSummary(
    string Id,
    string Name,
    string City,
    string Country,
    int StarRating,
    string ImageUrl,
    decimal StartingPrice,
    List<string> Amenities
);
