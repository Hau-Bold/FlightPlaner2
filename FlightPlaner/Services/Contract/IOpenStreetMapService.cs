using FlightPlaner.Models;
using FlightPlaner.Models.Domain;

namespace FlightPlaner.Services.Contract
{
    public interface IOpenStreetMapService
    {
        public Task<GPS> GetCoordinates(GPSRequestDTO request);
    }
}
