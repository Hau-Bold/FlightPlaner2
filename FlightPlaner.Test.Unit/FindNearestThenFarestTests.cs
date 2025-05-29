using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Impl;
using FlightPlaner.Services.Impl.Algorithms;

namespace FlightPlaner.Test.Unit;

using static FlightPlaner.Test.sdk.GpsTestHelper;

[TestFixture]
public class FindNearestThenFarestTests
{
    [Test]
    public void Execute_SingleTarget_ReturnsStartAndTarget()
    {
        var result = FindFarest.Execute(Berlin, [Bogota]);

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
        var result = FindFarest.Execute(Berlin, []);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(Berlin));
        });
    }

    [TestCaseSource(nameof(GetRoutes))]
    public void Execute_AlternatesBetweenNearestAndFarthestDistances(GPSDb start, List<GPSDb> targets)
    {
        var result = FindNearestThenFarest.Execute(start, targets);
        var distances = new List<double>();

        for (int i = 1; i < result.Count; i++)
        {
            distances.Add(GPSHelper.DistanceBetween(result[i - 1], result[i]));
        }

        for (int i = 0; i < distances.Count - 1; i++)
        {
            double current = distances[i];
            double next = distances[i + 1];

            if (i % 2 == 0) // Even index: nearest
            {
                Assert.That(current, Is.LessThanOrEqualTo(next),
                    $"Step {i}: expected NEAREST (distance {current}) to be <= next step (distance {next})");
            }
            else // Odd index: farthest
            {
                Assert.That(current, Is.GreaterThanOrEqualTo(next),
                    $"Step {i}: expected FARTHEST (distance {current}) to be >= next step (distance {next})");
            }
        }
    }

    private static IEnumerable<TestCaseData> GetRoutes()
    {
        yield return new TestCaseData(
            Berlin,
            new List<GPSDb> { Ankara, London, Canberra, NewDelhi,Moscow,Bogota }
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
