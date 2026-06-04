namespace HotelBooking.Api.Models;

// Hotel is a class (not a record) so EF Core can materialize it through a
// parameterless constructor and set the Rooms navigation collection via the
// property setter. The positional convenience constructor keeps existing
// `new(...)` call sites in SeedData unchanged.
public class Hotel
{
    public Hotel() { }

    public Hotel(
        string id,
        string name,
        string description,
        string city,
        string country,
        string address,
        double latitude,
        double longitude,
        int starRating,
        string imageUrl,
        List<string> amenities,
        List<Room> rooms)
    {
        Id = id;
        Name = name;
        Description = description;
        City = city;
        Country = country;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        StarRating = starRating;
        ImageUrl = imageUrl;
        Amenities = amenities;
        Rooms = rooms;
    }

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int StarRating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Amenities { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
}

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
