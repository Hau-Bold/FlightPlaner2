using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Impl.Algorithms;
using NUnit.Framework;
using System.Collections.Generic;

namespace FlightPlaner.Test.Unit;

using static FlightPlaner.Test.sdk.GpsTestHelper;

[TestFixture]
public class FindOptimizedTests
{
    [Test]
    public void Execute_SingleTarget_ReturnsStartAndTarget()
    {
        var result = FindOptimized.Execute(Berlin, [Bogota]);

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
        var result = FindOptimized.Execute(Berlin, []);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(Berlin));
        });
    }

    [TestCaseSource(nameof(GetRoutes))]
    public void Execute_WithMultipleTargets_YieldsExpectedRoute(GPSDb start, List<GPSDb> targets, List<GPSDb> expected, string description)
    {
        Assert.That(expected, Has.Count.EqualTo(targets.Count + 1));

        // Act
        var result = FindOptimized.Execute(start, targets);

        // Assert
        CollectionAssert.AreEqual(expected, result, $"Route computation failed for: {description}");
    }

    private static IEnumerable<TestCaseData> GetRoutes()
    {
        yield return new TestCaseData(
           Berlin,
           new List<GPSDb> { Bogota, Ankara, London },
           new List<GPSDb> { Berlin, Ankara, London, Bogota },
       "Berlin_To_Bogota_Ankara_London");

        yield return new TestCaseData(
            Tokyo,
            new List<GPSDb> { Canberra, NewDelhi, Moscow, Paris },
            new List<GPSDb> { Tokyo, Canberra, NewDelhi, Moscow, Paris },
        "Tokyo_To_Canberra_NewDelhi_Moscow_Paris");

        yield return new TestCaseData(
            Washington,
            new List<GPSDb> { Berlin, London, Paris, Bogota },
            new List<GPSDb> { Washington, Bogota, London, Paris, Berlin },
        "Washington_To_Berlin_London_Paris_Bogota");
    }
}
