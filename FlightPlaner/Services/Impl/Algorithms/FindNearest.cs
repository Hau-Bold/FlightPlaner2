using FlightPlaner.Models.Domain;

namespace FlightPlaner.Services.Impl.Algorithms
{
    public class FindNearest
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
                int nearestIndex = 0;
                double minDistance = double.MaxValue;

                for (int i = 0; i < remainingTargets.Count; i++)
                {
                    double distance = GPSHelper.DistanceBetween(start, remainingTargets[i]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestIndex = i;
                    }
                }

                GPSDb nearest = remainingTargets[nearestIndex];
                result.Add(nearest);
                start = nearest;
                remainingTargets.RemoveAt(nearestIndex);
            }

            result.AddRange(remainingTargets); // the last remaining target
            return result;
        }
    }
}
