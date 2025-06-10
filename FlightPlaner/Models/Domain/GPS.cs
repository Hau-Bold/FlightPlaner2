using System.Text.Json.Serialization;

namespace FlightPlaner.Models.Domain
{
    public class GPS
    {
        [JsonPropertyName("lat")]
        required public string Lat { get; set; }

        [JsonPropertyName("lon")]
        required public string Lon { get; set; }
    }
}
