using FlightPlaner.Models.Domain;
using FlightPlaner.Services.Contract;
using FlightPlaner.Services.Impl.Algorithms;

namespace FlightPlaner.Services.Impl;

public class OptimizationService(IRandomProvider randomProvider) : IOptimizationService
{
    public List<GPSDb> Compute(GPSDb start, List<GPSDb> targets,Algorithm algorithm)
    {
        return algorithm switch
        {
            Algorithm.Optimize => FindOptimized.Execute(start, targets),
            Algorithm.Nearest => FindNearest(start, targets),
            Algorithm.Farthest => FindFarest.Execute(start, targets),
            Algorithm.NearestFarthest => FindNearestThenFarest(start, targets),
            Algorithm.FarthestNearest => FindFarestThenNearest(start, targets),
            Algorithm.Random => new FindRandom(randomProvider).Execute(start, targets),
            _ => throw new NotImplementedException(),
        };
    }

    internal static List<GPSDb> FindNearest(GPSDb start, List<GPSDb> targets)
    {
        List<GPSDb> computedGPSCoordinates = [];
        List<GPSDb> copyOfTargets = new(targets);
        GPSDb copyOfStart = start;

        if (copyOfTargets.Count == 1)
        {
            computedGPSCoordinates.Add(start);
            computedGPSCoordinates.AddRange(copyOfTargets);
        }
        else
        {
            List<double> distanceList = [];
            computedGPSCoordinates.Add(start);
            while (copyOfTargets.Count > 1)
            {
                copyOfTargets.ForEach(gps => distanceList.Add(GPSHelper
                        .DistanceBetween(copyOfStart, gps)));

                DistanceAndIndex distanceAndIndex = GPSHelper.GetMinAndIndex(distanceList);
                int index = distanceAndIndex.Index;
                computedGPSCoordinates.Add(copyOfTargets[index]);
                start = copyOfTargets[index];
                copyOfTargets.RemoveAt(index);
                distanceList.Clear();
            }
            // to add the remaining
            copyOfTargets.ForEach(computedGPSCoordinates.Add);
        }

        return computedGPSCoordinates;
    }

    internal static List<GPSDb> FindNearestThenFarest(GPSDb start, List<GPSDb> targets)
    {
        List<GPSDb> computedGPSCoordinates = [];
        List<GPSDb> copyOfTargets = new(targets);
        GPSDb copyOfStart = start;

        if (copyOfTargets.Count == 1)
        {
            computedGPSCoordinates.Add(start);
            computedGPSCoordinates.AddRange(copyOfTargets);
        }
        else
        {
            computedGPSCoordinates.Add(start);

            List<double> distanceList = [];

            while (copyOfTargets.Count > 1)
            {

                copyOfTargets
                        .ForEach(gps => distanceList.Add(GPSHelper.DistanceBetween(copyOfStart, gps)));

                DistanceAndIndex point = GPSHelper.GetMinAndIndex(distanceList);

                int index = point.Index;
                computedGPSCoordinates.Add(copyOfTargets[index]);
                start = copyOfTargets[index];
                copyOfTargets.RemoveAt(index);
                distanceList.Clear();

                copyOfTargets
                        .ForEach(gps => distanceList.Add(GPSHelper.DistanceBetween(copyOfStart, gps)));

                point = GPSHelper.GetMaxAndIndex(distanceList);

                index = point.Index;
                computedGPSCoordinates.Add(copyOfTargets[index]);
                start = copyOfTargets[index];
                copyOfTargets.RemoveAt(index);
                distanceList.Clear();

            }

            copyOfTargets.ForEach(computedGPSCoordinates.Add);
        }

        return computedGPSCoordinates;
    }

    internal static List<GPSDb> FindFarestThenNearest(GPSDb start, List<GPSDb> targets)
    {
        List<GPSDb> computedGPSCoordinates = [];
        List<GPSDb> copyOfTargets = new(targets);
        GPSDb copyOfStart = start;

        if (copyOfTargets.Count == 1)
        {
            computedGPSCoordinates.Add(start);
            computedGPSCoordinates.AddRange(copyOfTargets);
        }
        else
        {
            computedGPSCoordinates.Add(start);

            List<double> distanceList = [];

            while (copyOfTargets.Count > 1)
            {

                copyOfTargets
                        .ForEach(gps => distanceList.Add(GPSHelper.DistanceBetween(copyOfStart, gps)));

                DistanceAndIndex point = GPSHelper.GetMaxAndIndex(distanceList);

                int index = point.Index;
                computedGPSCoordinates.Add(copyOfTargets[index]);
                start = copyOfTargets[index];
                copyOfTargets.RemoveAt(index);
                distanceList.Clear();

                copyOfTargets
                        .ForEach(gps => distanceList.Add(GPSHelper.DistanceBetween(copyOfStart, gps)));

                point = GPSHelper.GetMinAndIndex(distanceList);

                index = point.Index;
                computedGPSCoordinates.Add(copyOfTargets[index]);
                start = copyOfTargets[index];
                copyOfTargets.RemoveAt(index);
                distanceList.Clear();

            }

            copyOfTargets.ForEach(computedGPSCoordinates.Add);
        }

        return computedGPSCoordinates;
    }

    internal static List<GPSDb> FindOptimized1(GPSDb start, List<GPSDb> targets)
    {
        List<GPSDb> computedGPSCoordinates = [];
        List<GPSDb> copyOfTargets = new(targets);

        if (targets.Count == 1)
        {
            computedGPSCoordinates.Add(start);
            computedGPSCoordinates.AddRange(copyOfTargets);
        }
        else
        {
            List<List<GPSDb>> input = GPSHelper.GetPermutations(copyOfTargets);
            List<double> distances = [];

            input.ForEach(list =>
            {
                list.Insert(0, start);
                distances.Add(GPSHelper.GetTotalDistance(list));
            });

            DistanceAndIndex point = GPSHelper.GetMinAndIndex(distances);
            computedGPSCoordinates = new List<GPSDb>(input[point.Index]);
        }

        return computedGPSCoordinates;
    }

    

    public List<GPSDb> OptimizeWithSimulatedAnnealing(GPSDb start, List<GPSDb> targets)
    {
        // Initializing the route with the start point and shuffled targets
        List<GPSDb> currentRoute = [start, .. targets];

        // Initializing the best route with the current route as the best so far
        List<GPSDb> bestRoute = new(currentRoute);

        // Initializing the initial temperature and cooling rate for simulated annealing
        double temperature = 10000.0; // Initial temperature
        double coolingRate = 0.995;   // Cooling rate per iteration
        double absoluteTemperature = 0.01; // Threshold for stopping the algorithm

        // Loop until the temperature drops below the threshold
        while (temperature > absoluteTemperature)
        {
            // Generate a neighbor route by swapping two random cities (indices)
            List<GPSDb> newRoute = new(currentRoute);
            SwapTwoTargets(newRoute);

            // Calculate the current and new route distances using GPSHelper
            double currentDistance = GPSHelper.GetTotalDistance(currentRoute);
            double newDistance = GPSHelper.GetTotalDistance(newRoute);

            // If the new route is better, accept it
            if (newDistance < currentDistance)
            {
                currentRoute = new List<GPSDb>(newRoute);
            }
            else
            {
                // Otherwise, accept the new route with a probability based on temperature
                double acceptanceProbability = CalculateAcceptanceProbability(currentDistance, newDistance, temperature);
                if (randomProvider.NextDouble() < acceptanceProbability)
                {
                    currentRoute = new List<GPSDb>(newRoute);
                }
            }

            // If the new route is better, update the best route
            if (newDistance < GPSHelper.GetTotalDistance(bestRoute))
            {
                bestRoute = new List<GPSDb>(newRoute);
            }

            // Cool down the system (reduce temperature)
            temperature *= coolingRate;
        }

        return bestRoute;
    }

    //Continue with Genetic Algorithm

    private void SwapTwoTargets(List<GPSDb> route)
    {
        int idx1 = randomProvider.Next(route.Count);
        int idx2 = randomProvider.Next(route.Count);

        // Ensure the two indices are different
        while (idx1 == idx2)
        {
            idx2 = randomProvider.Next(route.Count);
        }

        // Swap the targets
        (route[idx2], route[idx1]) = (route[idx1], route[idx2]);
    }

    private static double CalculateAcceptanceProbability(double currentDistance, double newDistance, double temperature)
    {
        // Calculate the acceptance probability based on the Metropolis criterion
        if (newDistance < currentDistance)
        {
            return 1.0; // Always accept if the new route is better
        }
        return Math.Exp((currentDistance - newDistance) / temperature);
    }

}







