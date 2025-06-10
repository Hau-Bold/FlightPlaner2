using FlightPlaner.Models;
using FlightPlaner.Services.Contract;
using FlightPlaner.Test.sdk;
using Moq;
using System.Net.Http.Json;

namespace FlightPlaner.Test.Int
{
    [TestFixture]
    public class GPSControllerTests
    {
        [TestCaseSource(nameof(GetCoordinates))]
        public async Task AddCoordinate_ReturnsExpectedGps(GPSRequestDTO gpsRequestDTO, Models.Domain.GPS expected)
        {
            var mockOpenStreetMapService = new Mock<IOpenStreetMapService>();

            mockOpenStreetMapService.Setup(m => m.GetCoordinates(It.Is<GPSRequestDTO>(
                                     dto => AreEquivalent(dto, gpsRequestDTO))))
                                    .ReturnsAsync(expected);

            using var factory = new CustomWebApplicationFactory(
                Mock.Of<IOptimizationService>(),
                mockOpenStreetMapService.Object);

            using var client = factory.StartApplication();

            // Act
            var response = await client.PostAsJsonAsync("/api/GPS", gpsRequestDTO);

            // Assert
            response.EnsureSuccessStatusCode();
            var actual = await response.Content.ReadFromJsonAsync<Models.Domain.GPS>();

            Assert.That(actual, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(actual.Lat, Is.EqualTo(expected.Lat));
                Assert.That(actual.Lon, Is.EqualTo(expected.Lon));
            });
        }

        private static IEnumerable<TestCaseData> GetCoordinates()
        {
            yield return new TestCaseData(new GPSRequestDTO()
            {
                City = "Berlin",
                Country = "Germany"
            },
            new Models.Domain.GPS()
            {
                Lat = "52.5200",
                Lon = "13.4050",
            });
        }

        private static bool AreEquivalent(GPSRequestDTO actual, GPSRequestDTO expected)
        {
            return actual.City == expected.City &&
                   actual.Country == expected.Country &&
                   actual.Street == expected.Street &&
                   actual.PostalCode == expected.PostalCode &&
                   actual.IsStart == expected.IsStart;
        }
    }
}
