namespace FlightPlaner.Services.Impl
{
    public class DistanceAndIndex
    {
        public int Index { get; set; }
        public double Distance{ get; set; }

        public DistanceAndIndex(int index, double distance)
        {
            Index = index;
            Distance = distance;
        }
    }
}
