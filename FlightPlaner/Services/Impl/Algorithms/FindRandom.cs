using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Contract;

namespace FlightPlaner.Services.Impl.Algorithms
{
    public class FindRandom(IRandomProvider randomProvider)
    {
        internal List<GPSDb> Execute(GPSDb start, List<GPSDb> targets)
        {
            List<GPSDb> computedGPSCoordinates = [];

            // If no targets, just add the start GPS point
            if (targets.Count == 0)
            {
                computedGPSCoordinates.Add(start);
            }
            else
            {
                int[] indicesOfGPSCoordinates = new int[targets.Count];
                randomProvider.Generate(indicesOfGPSCoordinates, 0);

                for (int i = 0; i < indicesOfGPSCoordinates.Length; i++)
                {
                    computedGPSCoordinates.Add(targets[indicesOfGPSCoordinates[i]]);
                }

                computedGPSCoordinates.Insert(0, start);
            }

            return computedGPSCoordinates;
        }
    }
}
