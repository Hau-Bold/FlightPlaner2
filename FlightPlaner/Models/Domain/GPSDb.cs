using System.ComponentModel.DataAnnotations;

namespace FlightPlaner.Models.Domain;
public class GPSDb
{
    [Key]
    public Guid Guid { get; set; }
    public required string Lat { get; set; }
    public required string Lon { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public bool IsStart { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is GPSDb db &&
               Guid.Equals(db.Guid) &&
               Lat == db.Lat &&
               Lon == db.Lon &&
               City == db.City &&
               Country == db.Country &&
               Street == db.Street &&
               PostalCode == db.PostalCode &&
               IsStart == db.IsStart;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Guid, Lat, Lon, City, Country, Street, PostalCode, IsStart);
    }
}
