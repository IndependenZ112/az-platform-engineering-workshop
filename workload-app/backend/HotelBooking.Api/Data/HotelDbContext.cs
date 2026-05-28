using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Data;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Hotel>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Id).HasMaxLength(50);
            e.Property(h => h.Name).HasMaxLength(200);
            e.Property(h => h.City).HasMaxLength(100);
            e.Property(h => h.Country).HasMaxLength(100);
            e.Property(h => h.Address).HasMaxLength(300);
            e.Property(h => h.ImageUrl).HasMaxLength(500);
            // Amenities (List<string>) -> JSON column (EF Core 8+ primitive collection)
            e.HasMany(h => h.Rooms).WithOne().HasForeignKey(r => r.HotelId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Room>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasMaxLength(50);
            e.Property(r => r.HotelId).HasMaxLength(50);
            e.Property(r => r.Type).HasMaxLength(100);
            e.Property(r => r.Description).HasMaxLength(500);
            e.Property(r => r.PricePerNight).HasPrecision(10, 2);
        });

        b.Entity<Booking>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(50);
            e.Property(x => x.HotelId).HasMaxLength(50);
            e.Property(x => x.RoomId).HasMaxLength(50);
            e.Property(x => x.GuestName).HasMaxLength(200);
            e.Property(x => x.GuestEmail).HasMaxLength(200);
            e.Property(x => x.TotalPrice).HasPrecision(10, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.GuestEmail);
        });
    }
}
