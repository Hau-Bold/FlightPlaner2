using FlightPlaner.Models.Domain;
using FlightPlaner.Test.sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlaner.Test.Unit
{
    [TestFixture]
    public class FindFarestTests
    {
        private readonly GPSDb berlin = GpsTestHelper.CreateGPS(lat:"52.5200",lon: "13.4050",city: "Berlin",country: "Germany", isStart: true);
        private readonly GPSDb bogota = GpsTestHelper.CreateGPS(lat: "4.7110", lon: "-74.0721", city: "Bogotá", country: "Colombia");
        private readonly GPSDb ankara = GpsTestHelper.CreateGPS(lat: "39.9208", lon: "32.8541", city: "Ankara",country: "Turkey");
        private readonly GPSDb london = GpsTestHelper.CreateGPS(lat: "51.5074", lon: "-0.1278", city: "London", country: "United Kingdom");
        private readonly GPSDb paris = GpsTestHelper.CreateGPS(lat: "48.8566", lon: "2.3522", city: "Paris", country: "France");
        private readonly GPSDb washington = GpsTestHelper.CreateGPS(lat: "38.9072", lon: "-77.0369", city: "Washington", country: "United States");
        private readonly GPSDb tokyo = GpsTestHelper.CreateGPS(lat: "35.6762", lon: "139.6503", city: "Tokyo", country: "Japan");
        private readonly GPSDb canberra = GpsTestHelper.CreateGPS(lat: "-35.2809", lon: "149.1300", city: "Canberra", country: "Australia");
        private readonly GPSDb newDelhi = GpsTestHelper.CreateGPS(lat: "28.6139", lon: "77.2090", city: "New Delhi", country: "India");
        private readonly GPSDb moscow = GpsTestHelper.CreateGPS(lat: "55.7558", lon: "37.6173", city: "Moscow", country: "Russia");

    }
}
