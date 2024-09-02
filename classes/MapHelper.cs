using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.SignalR;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;


namespace Heave
{
    public class MapHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<CoordinateHub> _hubContext;
        public MapHelper(IConfiguration configuration, IHubContext<CoordinateHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }
        public static List<Coordinate> GetPointsAlongLine(List<Coordinate> coList, Coordinate start, Coordinate end, double divisor)
        {
            //LineString? line = new LineString(new Coordinate[] { start, end });
            LineString? line = new LineString(coList.ToArray());

            double length = line.Length;
            double interval = length / divisor;
            List<Coordinate>? points = new List<Coordinate>();
            for (double dist = 0; dist <= length; dist += interval)
            {
                Coordinate? extractPoint = new LengthIndexedLine(line).ExtractPoint(dist);
                points.Add(extractPoint);
            }
            return points;
        }

        public void DronePing(List<Coordinate> flightPath)
        {
            Coordinate? start = new Coordinate(49.496280006567815, -117.34430314398065);
            Coordinate? end = new Coordinate(49.48819605167058, -117.28473664866941);
            List<Coordinate>? points = GetPointsAlongLine(flightPath, start, end, 30); //start, end, number of intervals
            ProgressReporter? reporter = new ProgressReporter(1000, points, _hubContext);
            reporter.ProgressChanged += Reporter_ProgressChanged;
            reporter.Start();
        }
        static void Reporter_ProgressChanged(object sender, ProgressEventArgs e)
        {
            Console.WriteLine(e.Message); // Handle the progress update
        }


        public List<Node> PathToMap(string customerId)     //creates a dijkstra path and outputs it in json for Leaflet mapping
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            Program.GeoConnect geoConnect = new();
            geoConnect.SqlStr = connectionString;
            List<Node> nodeList = geoConnect.DBGetGraphData(Convert.ToInt32(customerId));
            foreach(Node node in nodeList)
            {
                System.Console.WriteLine($"PathToMap node geo for {node.Id} is {node.GeoPoint}");
            }
            Dijkstra? dijkstra = new Dijkstra();
            Node startingNode = nodeList.FirstOrDefault(node => node.Id == "n1");
            List<Node> pathNodeList = dijkstra.ExecuteDij(startingNode, nodeList, customerId);//obtain flight path points
            foreach(Node node in pathNodeList){System.Console.WriteLine($"Raw Dijk List {node.Id}");}
            return pathNodeList;
            /*List<Coordinate> pathPoints = geoConnect.NodesToCoordinates(pathNodeList);
            DronePing(pathPoints);//Start drone dummy along the path
            string jsonString = geoConnect.ConvertCoordsToGeoJson(pathPoints);
            return jsonString;*/
        }
    }
}