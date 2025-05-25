using FlightPlaner.Models.Domain;
using System.Globalization;

namespace FlightPlaner.Test.sdk
{
    internal static class GpsTestHelper
    {
        public static GPSDb CreateGPS(string lat,string lon,string city,string country, bool isStart = false) => new()
        {
            Guid = Guid.NewGuid(),
            Lat = lat,
            Lon = lon,
            City = city,
            Country = country,
            IsStart = isStart
        };
    }
}
