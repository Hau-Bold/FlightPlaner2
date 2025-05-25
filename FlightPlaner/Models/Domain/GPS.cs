using System.Text.Json.Serialization;

namespace FlightPlaner.Models.Domain
{
    public record GPS
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        [JsonPropertyName("lon")]
        public string Lon { get; set; }
    }
}
