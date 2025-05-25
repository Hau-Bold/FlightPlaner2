using FlightPlaner.Services.Contract;

namespace FlightPlaner.Services.Impl;

public class RandomProvider : IRandomProvider
{
    private readonly Random random;

    public RandomProvider() 
        => random = new Random();

    public int Next(int maxValue)
        => random.Next(maxValue);

    public double NextDouble()
      => random.NextDouble();

    public void Generate(int[]? values, int currentIndex)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Values array must not be null or empty.", nameof(values));

        if (values.Length == 1)
        {
            return;
        }

        values[currentIndex] = random.Next(values.Length);

        for (int i = 0; i <currentIndex; i++)
        {
            if (values[i] == values[currentIndex])
            {
                Generate(values, currentIndex);

                return;
            }
        }

        if (currentIndex < values.Length - 1)
        {
            // generate next index
            Generate(values, currentIndex + 1);
        }
    }
}
