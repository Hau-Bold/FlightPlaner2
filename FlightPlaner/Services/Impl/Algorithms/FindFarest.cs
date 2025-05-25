using FlightPlaner.Models.Domain;

namespace FlightPlaner.Services.Impl.Algorithms
{
    public class FindFarest
    {
        internal static List<GPSDb> Execute(GPSDb start, List<GPSDb> targets)
        {
            List<GPSDb> result = [start];

            if (targets.Count == 1)
            {
                result.AddRange(targets);
                return result;
            }

            List<GPSDb> remainingTargets = new(targets);

            while (remainingTargets.Count > 1)
            {
                int farthestIndex = 0;
                double maxDistance = double.MinValue;

                for (int i = 0; i < remainingTargets.Count; i++)
                {
                    double distance = GPSHelper.DistanceBetween(start, remainingTargets[i]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestIndex = i;
                    }
                }

                GPSDb farthest = remainingTargets[farthestIndex];
                result.Add(farthest);
                start = farthest;
                remainingTargets.RemoveAt(farthestIndex);
            }

            result.AddRange(remainingTargets);
            return result;
        }
    }
}
