using FlightPlaner.Models.Domain;
using FlightPlaner.Test.sdk;
using FlightPlaner.Services.Contract;
using FlightPlaner.Services.Impl.Algorithms;
using Moq;

namespace FlightPlaner.Test.Unit;

[TestFixture]
public class FindRandomTests
{
    private Mock<IRandomProvider> randomProviderMock;
    private FindRandom sut;

    [SetUp]
    public void SetUp()
    {
        randomProviderMock = new Mock<IRandomProvider>();

        // Mock: reverse order for 3 elements => indices [2,1,0]
        randomProviderMock.Setup(r => r.Generate(It.IsAny<int[]>(), 0))
                  .Callback<int[], int>((arr, _) =>
                  {
                      for (int i = 0; i < arr.Length; i++)
                      {
                          arr[i] = arr.Length - 1 - i;
                      }
                  });

        sut = new FindRandom(randomProviderMock.Object);
    }

    [Test]
    public void FindRandom_ShouldReturnStartFollowedByShuffledTargets()
    {
        // Arrange
        var start = GpsTestHelper.CreateGPS("13.4","47.12", "StartCity", "aCountry", isStart: true);
        var targets = new List<GPSDb>
        {
             GpsTestHelper.CreateGPS("52.0", "13.0", "A", "aCountry"),
             GpsTestHelper.CreateGPS("52.0", "13.1", "B", "aCountry"),
             GpsTestHelper.CreateGPS("52.0", "13.2", "C", "aCountry")
        };

        // Act
        var result = sut.Execute(start, targets);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.First().City, Is.EqualTo("StartCity"));

            var shuffled = result.Skip(1).ToList();
            Assert.That(shuffled[0].City, Is.EqualTo("C"));
            Assert.That(shuffled[1].City, Is.EqualTo("B"));
            Assert.That(shuffled[2].City, Is.EqualTo("A"));

            CollectionAssert.AreEquivalent(
                targets.Select(g => g.Guid),
                shuffled.Select(g => g.Guid)
            );
        });
    }

    [Test]
    public void FindRandom_ShouldReturnStartAndOnlyTarget_WhenSingleTargetProvided()
    {
        // Arrange
        var start = GpsTestHelper.CreateGPS( "52.0", "13.0", "StartCity","aCountry", isStart: true);
        var singleTarget = GpsTestHelper.CreateGPS("52.0","13.7", "TargetCity","aCountry");

        var targets = new List<GPSDb> { singleTarget };

        // Act
        var result = sut.Execute(start, targets);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].City, Is.EqualTo("StartCity"));
            Assert.That(result[1].Guid, Is.EqualTo(singleTarget.Guid));
        });

        // Ensure Generate is not called
        randomProviderMock.Verify(r => r.Generate(It.IsAny<int[]>(), It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void FindRandom_ShouldReturnOnlyStart_WhenTargetsAreNotProvided()
    {
        // Arrange
        var start = GpsTestHelper.CreateGPS( "52.0", "13.0", "StartCity", "aCountry", isStart: true);
        var targets = new List<GPSDb>();

        // Act
        var result = sut.Execute(start, targets);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].City, Is.EqualTo("StartCity"));

        // Ensure Generate is not called
        randomProviderMock.Verify(r => r.Generate(It.IsAny<int[]>(), It.IsAny<int>()), Times.Never);
    }
}
