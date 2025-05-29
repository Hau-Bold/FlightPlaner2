using System.Text.Json.Serialization;

namespace FlightPlaner.Models.Domain
{
    public record Point
    {
        [JsonPropertyName("xPx")]
        public int X { get; set; }

        [JsonPropertyName("yPx")]
        public int Y { get; set; }

        public bool IsStart { get; set; }

        required public string City { get; set; }
    }
}
