using FlightPlaner.Data;
using FlightPlaner.Models;
using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Contract;
using FlightPlaner.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlightPlaner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GPSController(GPSDbContext context, IOpenStreetMapService openStreetMapService, IOptimizationService optimizationService) : Controller
    {
        public GPSDbContext DbContext { get; } = context;

        [HttpGet]
        public IActionResult GetCoordinates()
        {
           var coordinates = DbContext.Coordinates.ToList();

            var startCoordinate = coordinates.FirstOrDefault(c => c.IsStart);

            if (startCoordinate != null)
            {
                coordinates.Remove(startCoordinate);
                coordinates.Insert(0, startCoordinate); // Start goes first
            }

            return Ok(coordinates);
        }


        [HttpGet]
        [Route("GetMillerCoordinates")]
        public IActionResult GetMillerCoordinates([FromQuery] int actualWidth, [FromQuery] int actualHeight)
        {
            var coordinates = DbContext.Coordinates.ToList();

            if (coordinates == null || coordinates.Count == 0)
            {
                return NotFound("No coordinates found.");
            }

            var millerCoordinates = coordinates
                .Select(gps => GPSHelper.ToMiller(gps, actualWidth, actualHeight))
                .ToList();

            return Ok(millerCoordinates);
        }

        [HttpGet]
        [Route("GetOptimizedCoordinates")]
        public IActionResult GetOptimizedCoordinates([FromQuery] Algorithm algorithm)
        {
            var coordinates = DbContext.Coordinates.ToList();

            if (coordinates == null || coordinates.Count == 0)
            {
                return NotFound("No coordinates found.");
            }

            var startCoordinate = coordinates.Single(gps => gps.IsStart);
            coordinates.Remove(startCoordinate);

            var optimizedCoordinates =optimizationService.Compute(startCoordinate,coordinates,algorithm);

            return Ok(optimizedCoordinates);
        }

        [HttpGet]
        [Route("GetOptimizedMillerCoordinates")]
        public IActionResult GetOptimizedMillerCoordinates([FromQuery] int actualWidth, [FromQuery] int actualHeight,[FromQuery] Algorithm algorithm)
        {
            var coordinates = DbContext.Coordinates.ToList();

            if (coordinates == null || coordinates.Count == 0)
            {
                return NotFound("No coordinates found.");
            }

            var startCoordinate = coordinates.Single(gps => gps.IsStart);
            coordinates.Remove(startCoordinate);

            var optimizedCoordinates = optimizationService.Compute(startCoordinate, coordinates, algorithm);

            var millerCoordinates = coordinates
             .Select(gps => GPSHelper.ToMiller(gps, actualWidth, actualHeight))
             .ToList();

            return Ok(millerCoordinates);
        }

        [HttpPost]
        public async Task<IActionResult> Add(GPSRequestDTO dto)
        {
            var result = await openStreetMapService.GetCoordinates(dto);

            // Normalize input for comparison
            var normStreet = (dto.Street ?? string.Empty).Trim().ToLower();
            var normCity = (dto.City ?? string.Empty).Trim().ToLower();
            var normPostal = (dto.PostalCode ?? string.Empty).Trim().ToLower();
            var normCountry = (dto.Country ?? string.Empty).Trim().ToLower();
            var isStart = dto.IsStart;

            // Check for duplicate entry
            var duplicateExists = await DbContext.Coordinates.AnyAsync(g =>
                g.Street.Trim().ToLower() == normStreet &&
                g.City.Trim().ToLower() == normCity &&
                g.PostalCode.Trim().ToLower() == normPostal &&
                g.Country.Trim().ToLower() == normCountry &&
                g.IsStart == isStart
            );

            if (duplicateExists)
            {
                return Conflict("This coordinate already exists.");
            }

            var gpsData = new GPSDb
            {
                Guid = Guid.NewGuid(),
                Lat = result.Lat,
                Lon = result.Lon,
                Street = dto.Street ?? string.Empty,
                City = dto.City,
                PostalCode = dto.PostalCode ?? string.Empty,
                Country = dto.Country,
                IsStart = dto.IsStart,
            };

            if (dto.IsStart)
            {
                var existingStart = await DbContext.Coordinates
                    .FirstOrDefaultAsync(g => g.IsStart);

                if (existingStart != null)
                {
                    existingStart.IsStart = false;
                    DbContext.Coordinates.Update(existingStart);
                }
            }

            await DbContext.Coordinates.AddAsync(gpsData);
            await DbContext.SaveChangesAsync();

            return Ok(result);
        }


        [HttpPost]
        [Route("{id:guid}")]
        public async Task<IActionResult> Update(GPSRequestDTO dto)
        {
            //from the url: get Coordinates!
            var result = await openStreetMapService.GetCoordinates(dto);

            // next: add the Result to the database
            var gpsData = new GPSDb
            {
                Guid = Guid.NewGuid(),
                Lat = result.Lat,
                Lon = result.Lon,
                Street = dto.Street ?? string.Empty,
                City = dto.City,
                PostalCode = dto.PostalCode ?? string.Empty,
                Country = dto.Country,
                IsStart = dto.IsStart,
            };

            DbContext.Coordinates.Add(gpsData);
            DbContext.SaveChanges();

            return Ok(result);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteCoordinate(Guid id)
        {
            var coordinate = DbContext.Coordinates.Find(id);
            if (coordinate is not null)
            {
                DbContext.Coordinates.Remove(coordinate);
                DbContext.SaveChanges();
            }

            return Ok();
        }
    }
}
