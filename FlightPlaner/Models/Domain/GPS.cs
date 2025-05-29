using System.Text.Json.Serialization;

namespace FlightPlaner.Models.Domain
{
    public record GPS
    {
        [JsonPropertyName("lat")]
        required public string Lat { get; set; }

        [JsonPropertyName("lon")]
        required public string Lon { get; set; }
    }
}
