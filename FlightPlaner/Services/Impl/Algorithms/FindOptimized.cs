using FlightPlaner.Models.Domain;

namespace FlightPlaner.Services.Impl.Algorithms
{
    public class FindOptimized
    {
        internal static List<GPSDb> Execute(GPSDb start, List<GPSDb> targets)
        {
            // Return early if targets list is null or empty
            if (targets == null || targets.Count == 0)
                return [start];

            // If only one target, return start + target
            if (targets.Count == 1)
                return [start, targets[0]];

            var permutations = GPSHelper.GetPermutations(targets);
            double minDistance = double.MaxValue;
            List<GPSDb>? bestRoute = null;

            foreach (var route in permutations)
            {
                // Prepend start point
                var fullRoute = new List<GPSDb>(route.Count + 1) { start };
                fullRoute.AddRange(route);

                var distance = GPSHelper.GetTotalDistance(fullRoute);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestRoute = fullRoute;
                }
            }

            return bestRoute ?? [start];
        }

    }
}
