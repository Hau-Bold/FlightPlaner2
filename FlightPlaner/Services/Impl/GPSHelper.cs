using FlightPlaner.Models.Domain;
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

        internal static int GetNearestIndex(GPSDb from, List<GPSDb> targets)
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

        internal static int GetFarthestIndex(GPSDb from, List<GPSDb> targets)
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

        internal static Point ToMercator(GPSDb gps, int actualWidth, int actualHeight, int imageWidth, int imageHeight)
        {
            // Map image size (fixed, based on your image)
            //const double imageWidth = 2048.0;
            //const double imageHeight = 1588.0;

            // Convert input GPS coordinates
            double lon = ToDouble(gps.Lon);
            double lat = ToDouble(gps.Lat);

            // Clamp latitude to prevent extreme distortion near poles
            if (lat > 89.5) lat = 89.5;
            if (lat < -89.5) lat = -89.5;

            // Convert to radians
            double latRad = ToRadians(lat);

            // Mercator projection (x from -180 to +180, y from +85 to -85 approximately)
            double xMercator = (lon + 180.0) / 360.0 * imageWidth;

            double yMercator = imageHeight / 2.0 -
                               (imageWidth / (2.0 * Math.PI)) *
                               Math.Log(Math.Tan(Math.PI / 4.0 + latRad / 2.0));

            // Scale from image space to canvas space
            double scaleX = (double)actualWidth / imageWidth;
            double scaleY = (double)actualHeight / imageHeight;

            int xPx = (int)Math.Truncate(xMercator * scaleX);
            int yPx = (int)Math.Truncate(yMercator * scaleY);

            return new Point
            {
                X = xPx,
                Y = yPx,
                IsStart = gps.IsStart,
                City = gps.City,
            };
        }



        private static double ToRadians(double degrees) 
            => degrees * Math.PI / 180.0;

        private static double ToDouble(string input) 
            => Convert.ToDouble(input, CultureInfo.InvariantCulture);
    }
}
