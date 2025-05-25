using FlightPlaner.Models.Domain;
using MathNet.Numerics.Random;
using System.Globalization;

namespace FlightPlaner.Services.Impl
{
    internal class GPSHelper
    {
         internal static double DistanceBetween(GPSDb from, GPSDb to)
        {
            double radius = 6371.0;

            double lonFrom = ToDouble(from.Lon);
            double latFrom = ToDouble(from.Lat);

            double lonTo = ToDouble(to.Lon);
            double latTo = ToDouble(to.Lat);

            double distance = Math.Sin(ToRadians(latFrom)) * Math.Sin(ToRadians(latTo));

            distance += Math.Cos(ToRadians(latFrom)) * Math.Cos(ToRadians(latTo))
                    * Math.Cos(ToRadians((lonTo - lonFrom)));
            distance = Math.Acos(distance);
            distance *= radius;
            return distance;
        }

        internal static double GetTotalDistance(List<GPSDb> targets)
        {
            double response = .0;
            for (int i = 0; i < targets.Count - 1; i++)
            {
                response += DistanceBetween(targets[i], targets[i + 1]);
            }
            return response;
        }

        internal static DistanceAndIndex GetMaxAndIndex(List<double> distances)
        {

            DistanceAndIndex distanceAndCorrespondingIndex = new(0, distances[0]);

            for (int i = 1; i < distances.Count; i++)
            {
                if (distances[i] >= distanceAndCorrespondingIndex.Distance)
                {
                    distanceAndCorrespondingIndex.Index =i;
                    distanceAndCorrespondingIndex.Distance =distances[i];
                }
            }
            return distanceAndCorrespondingIndex;
        }

        internal static DistanceAndIndex GetMinAndIndex(List<double> distances)
        {

            DistanceAndIndex distanceAndCorrespondingIndex = new(0, distances[0]);

            for (int i = 1; i < distances.Count; i++)
            {
                if (distances[i] <= distanceAndCorrespondingIndex.Distance)
                {
                    distanceAndCorrespondingIndex.Index=i;
                    distanceAndCorrespondingIndex.Distance=distances[i];
                }
            }
            return distanceAndCorrespondingIndex;
        }

        internal static List<List<GPSDb>> GetPermutations(List<GPSDb> targets)
        {
            List<List<GPSDb>> permutations = [];
            if (targets.Count == 2)
            {
                List<GPSDb> values1 = [];
                List<GPSDb> values2 = [];
                values1.Add(targets[0]);
                values1.Add(targets[1]);
                values2.Add(targets[1]);
                values2.Add(targets[0]);
                permutations.Add(values1);
                permutations.Add(values2);
            }
            else
            {
                foreach (GPSDb item in targets)
                {
                    List<GPSDb> copy = new(targets);
                    copy.Remove(item);
                    List<List<GPSDb>> perm = GetPermutations(copy);
                    foreach (List<GPSDb> p in perm)
                    {
                        copy = [item, .. p];
                        permutations.Add(copy);
                    }
                }
            }

            return permutations;
        }

        internal static Point ToMiller(GPSDb gps, int actualWidth, int actualHeight)
        {
            double lon = ToDouble(gps.Lon);
            double lat = ToDouble(gps.Lat);

            double xMiller = ((lon + 180.0) / 360.0) * actualWidth;

            double latInRadians = ToRadians(lat);
            double yMiller = (actualHeight / 2.0) -
                             (actualHeight / 4.0) *
                             (1.25 * Math.Log(Math.Tan(Math.PI / 4.0 + 0.4 * latInRadians)));

            int xPx = (int)Math.Truncate(xMiller);
            int yPx = (int)Math.Truncate(yMiller);

            return new Point
            {
                X = xPx,
                Y =yPx,
                IsStart = gps.IsStart,
                City=gps.City,
            };
        }

        private static double ToRadians(double degrees) 
            => degrees * Math.PI / 180.0;

        private static double ToDouble(string input) 
            => Convert.ToDouble(input, CultureInfo.InvariantCulture);
    }
}
