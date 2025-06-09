using FlightPlaner.Services.Impl;
using NUnit.Framework;
using System;
using System.Linq;

namespace FlightPlaner.Test.Unit
{
    public class RandomProviderTests
    {
        private RandomProvider testee;

        [SetUp]
        public void SetUp() => testee = new RandomProvider();

        [Test]
        public void Generate_ShouldFillArrayWithUniqueRandomValues()
        {
            // Arrange
            int length = 10;
            int[] values = new int[length];

            // Act
            testee.Generate(values, 0);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(values.Distinct().Count(), Is.EqualTo(length));
                Assert.That(values.All(v => v >= 0 && v < length), Is.True);
            });
        }

        [Test]
        public void Generate_ShouldThrowException_WhenArrayIsNull()
        {
            // Arrange
            var func = () => testee.Generate(null, 0);

            // Act && Assert
            Assert.Throws<ArgumentException>(func.Invoke);
        }

        [Test]
        public void Generate_ShouldThrowException_WhenArrayIsEmpty()
        {
            // Arrange
            var func = () => testee.Generate([], 0);

            // Act && Assert
            Assert.Throws<ArgumentException>(func.Invoke);
        }
    }
}
