namespace FlightPlaner.Services.Contract
{
    /// <summary>
    /// Interface for a random number provider that allows generating random integers, doubles, 
    /// and filling arrays with unique random values.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>
        /// Generates a random integer that is greater than or equal to 0 and less than the specified maximum value.
        /// </summary>
        /// <param name="maxValue">The upper bound (exclusive) for the random integer.</param>
        /// <returns>A random integer between 0 and <paramref name="maxValue"/> - 1.</returns>
        int Next(int maxValue);

        /// <summary>
        /// Generates a random floating-point number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A random double between 0.0 and 1.0.</returns>
        double NextDouble();

        /// <summary>
        /// Fills an array with unique random indices, starting from the specified index.
        /// </summary>
        /// <param name="values">The array of integers to be filled with unique random indices.</param>
        /// <param name="currentIndex">The current index to start filling from in the array.</param>
        /// <exception cref="ArgumentException">Thrown if the array is null or empty.</exception>
        void Generate(int[]? values, int currentIndex);
    }
}
