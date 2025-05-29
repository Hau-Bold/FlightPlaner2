using FlightPlaner.Models.Domain;

namespace FlightPlaner.Test.sdk;
internal static class GpsTestHelper
{
    internal static readonly GPSDb Berlin = CreateGPS("52.5200", "13.4050", "Berlin", "Germany", true);
    internal static readonly GPSDb Bogota = CreateGPS("4.7110", "-74.0721", "Bogotá", "Colombia");
    internal static readonly GPSDb Ankara = CreateGPS("39.9208", "32.8541", "Ankara", "Turkey");
    internal static readonly GPSDb London = CreateGPS("51.5074", "-0.1278", "London", "United Kingdom");
    internal static readonly GPSDb Paris = CreateGPS("48.8566", "2.3522", "Paris", "France");
    internal static readonly GPSDb Washington = CreateGPS("38.9072", "-77.0369", "Washington", "United States");
    internal static readonly GPSDb Tokyo = CreateGPS("35.6762", "139.6503", "Tokyo", "Japan");
    internal static readonly GPSDb Canberra = CreateGPS("-35.2809", "149.1300", "Canberra", "Australia");
    internal static readonly GPSDb NewDelhi = CreateGPS("28.6139", "77.2090", "New Delhi", "India");
    internal static readonly GPSDb Moscow = CreateGPS("55.7558", "37.6173", "Moscow", "Russia");

    internal static GPSDb CreateGPS(string lat, string lon, string city, string country, bool isStart = false) => new()
    {
        Guid = Guid.NewGuid(),
        Lat = lat,
        Lon = lon,
        City = city,
        Country = country,
        IsStart = isStart
    };
}
