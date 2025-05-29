using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Impl;
using FlightPlaner.Services.Impl.Algorithms;

namespace FlightPlaner.Test.Unit;

using static FlightPlaner.Test.sdk.GpsTestHelper;

[TestFixture]
public class FindNearestTests
{
    [Test]
    public void Execute_SingleTarget_ReturnsStartAndTarget()
    {
        var result = FindNearest.Execute(Berlin, [Bogota]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(Berlin));
            Assert.That(result[1], Is.EqualTo(Bogota));
        });
    }

    [Test]
    public void Execute_EmptyTargets_ReturnsStart()
    {
        var result = FindNearest.Execute(Berlin, []);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(Berlin));
        });
    }

    [TestCaseSource(nameof(GetRoutes))]
    public void Execute_WithMultipleTargets_ReturnsInCorrectOrder(GPSDb start, List<GPSDb> targets)
    {
        var result = FindNearest.Execute(start, targets);

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(start));
            Assert.That(result, Has.Count.EqualTo(targets.Count + 1));

            for (int i = 1; i < result.Count - 1; i++)
            {
                double prevDistance = GPSHelper.DistanceBetween(result[i - 1], result[i]);

                for (int j = i + 1; j < result.Count; j++)
                {
                    double compareDistance = GPSHelper.DistanceBetween(result[i - 1], result[j]);
                    Assert.That(prevDistance, Is.LessThanOrEqualTo(compareDistance),
                        $"Element at index {i} is not the nearest from previous.");
                }
            }
        });
    }

    private static IEnumerable<TestCaseData> GetRoutes()
    {
        yield return new TestCaseData(
            Berlin,
            new List<GPSDb> { Ankara, London, Canberra, NewDelhi }
        ).SetName("FromBerlin_ToEuropeAsiaAustralia");

        yield return new TestCaseData(
            Bogota,
            new List<GPSDb> { Washington, Moscow, Tokyo }
        ).SetName("FromBogota_ToUSA_Russia_Japan");

        yield return new TestCaseData(
            Paris,
            new List<GPSDb> { London, NewDelhi }
        ).SetName("FromParis_ToLondonAndDelhi");
    }
}
