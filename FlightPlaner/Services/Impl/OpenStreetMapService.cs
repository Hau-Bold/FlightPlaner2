using FlightPlaner.Models;
using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Contract;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FlightPlaner.Services.Impl
{
    public class OpenStreetMapService : IOpenStreetMapService
    {
        private static readonly string openStreetMapBaseAddress = "https://nominatim.openstreetmap.org";
        private static readonly string openStreetMapSpecAddress = "search?street={0}&city={1}&postalcode={2}&country={3}&format=json";
        private HttpClient myHttpClient;

        public OpenStreetMapService(HttpClient httpClient)
        {
            myHttpClient = httpClient;
            myHttpClient.BaseAddress = new Uri(openStreetMapBaseAddress);
            // Add a default User-Agent header to identify the application.
            //myHttpClient.DefaultRequestHeaders.Add("User-Agent", "FlightPlaner/1.0 (your-email@example.com)");
            myHttpClient.DefaultRequestHeaders.Add("User-Agent", "FlightPlaner/1.0 (Michael.Krasser@t-online.de)");
        }

        /// <inheritdoc />
        public async Task<GPS> GetCoordinates([NotNull] GPSRequestDTO requestDTO)
        {
            // Ensure proper URL encoding for each component of the address
            var formattedURL = string.Format(openStreetMapSpecAddress,
                Uri.EscapeDataString(requestDTO.Street??string.Empty),
                Uri.EscapeDataString(requestDTO.City),
                Uri.EscapeDataString(requestDTO.PostalCode??string.Empty),
                Uri.EscapeDataString(requestDTO.Country)
            );

            // Make the request
            return await GetRequest(formattedURL);
        }

        private async Task<GPS> GetRequest(string urlObject)
        {
            var response = await myHttpClient.GetAsync(urlObject);

            // Handle the response
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var data = JsonSerializer.Deserialize<GPS[]>(jsonResponse);

                if (!Comparer<GPS>.Default.Equals(data[0]))
                {
                    return data[0];
                }

                throw new InvalidOperationException($"Not able to parse from source {jsonResponse}");
            }
            else
            {
                // Log the error details (optional)
                Console.WriteLine($"Error: {response.StatusCode}");
                throw new InvalidOperationException($"Not able to get request from source {urlObject}");
            }
        }
    }
}
