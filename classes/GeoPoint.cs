namespace Heave
{
    public class GeoPoint
    {
        public string WKTPoint{ get; set;}
        public string PointName { get; set;}

        public GeoPoint(string pointName, string point)
        {
            WKTPoint = point;
            PointName = pointName;
        }
    }
}