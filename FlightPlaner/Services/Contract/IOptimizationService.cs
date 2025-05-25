using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Impl;

namespace FlightPlaner.Services.Contract
{
    public interface IOptimizationService
    {
        List<GPSDb> Compute(GPSDb start, List<GPSDb> targets, Algorithm algorithm);
    }
}
