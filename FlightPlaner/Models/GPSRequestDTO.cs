namespace FlightPlaner.Models
{
    public class GPSRequestDTO
    {
        public Guid Id { get; set; }

        public string? Street   { get; set; }

        public required string City { get; set; }

        public string? PostalCode { get; set; }

        public required string Country { get; set; }
        public bool IsStart { get; set; } = false;
    }
}

