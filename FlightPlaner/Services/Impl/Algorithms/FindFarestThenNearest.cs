using FlightPlaner.Models.Domain;

namespace FlightPlaner.Services.Impl.Algorithms;

internal class FindFarestThenNearest
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
            int farthestIndex  = GPSHelper.GetFarthestIndex(current, remaining);
            current = remaining[farthestIndex];
            result.Add(current);
            remaining.RemoveAt(farthestIndex);

            if (remaining.Count == 0)
            {
                break;
            }

            int nearestIndex = GPSHelper.GetNearestIndex(current, remaining);
            current = remaining[nearestIndex];
            result.Add(current);
            remaining.RemoveAt(nearestIndex);
        }

        if (remaining.Count == 1)
        {
            result.Add(remaining[0]);
        }

        return result;
    }
}
