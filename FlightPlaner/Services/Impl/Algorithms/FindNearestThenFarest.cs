using FlightPlaner.Models.Domain;

namespace FlightPlaner.Services.Impl.Algorithms;

internal class FindNearestThenFarest
{
    internal static List<GPSDb> Execute(GPSDb start, List<GPSDb> targets)
    {
        var result = new List<GPSDb> { start };

        if (targets == null || targets.Count == 0)
        {
            return result;
        }

        if (targets.Count == 1)
        {
            result.Add(targets[0]);
            return result;
        }

        var remaining = new List<GPSDb>(targets);
        var current = start;

        while (remaining.Count > 1)
        {
            int nearestIndex = GetNearestIndex(current, remaining);
            current = remaining[nearestIndex];
            result.Add(current);
            remaining.RemoveAt(nearestIndex);

            if (remaining.Count == 0)
            {
                break;
            }

            int farthestIndex = GetFarthestIndex(current, remaining);
            current = remaining[farthestIndex];
            result.Add(current);
            remaining.RemoveAt(farthestIndex);
        }

        if (remaining.Count == 1)
        {
            result.Add(remaining[0]);
        }

        return result;
    }

    private static int GetNearestIndex(GPSDb from, List<GPSDb> targets)
    {
        int index = 0;
        double minDistance = double.MaxValue;

        for (int i = 0; i < targets.Count; i++)
        {
            double distance = GPSHelper.DistanceBetween(from, targets[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }

        return index;
    }

    private static int GetFarthestIndex(GPSDb from, List<GPSDb> targets)
    {
        int index = 0;
        double maxDistance = double.MinValue;

        for (int i = 0; i < targets.Count; i++)
        {
            double distance = GPSHelper.DistanceBetween(from, targets[i]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                index = i;
            }
        }

        return index;
    }
}
