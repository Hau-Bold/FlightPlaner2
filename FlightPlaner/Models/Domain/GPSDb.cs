using System.ComponentModel.DataAnnotations;

namespace FlightPlaner.Models.Domain
{
    public record GPSDb
    {
        [Key]
        public Guid Guid { get; set; }
        public required string Lat { get; set; }
        public required string Lon { get; set; }
        public string Street { get; set; } = string.Empty;
        public required string City { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public required string Country { get; set; }
        public bool IsStart { get; set; }
    }
}
