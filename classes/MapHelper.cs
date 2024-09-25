using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.SignalR;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using Microsoft.AspNetCore.SignalR;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

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
        private Program.GeoConnect InitGeoConnect()//handles sql init for GeoConnect DB methods
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            Program.GeoConnect geoConnect = new();
            geoConnect.SqlStr = connectionString;
            return geoConnect;
        }

        public List<Node> PathToMap(string customerId)     //creates a dijkstra path and outputs it in json for Leaflet mapping
        {
            Program.GeoConnect geoConnect = InitGeoConnect();
            String startNode = "p12";
            List<Node> nodeList = geoConnect.DBGetGraphData(Convert.ToInt32(customerId));
            foreach (Node node in nodeList)
            {
                System.Console.WriteLine($"PathToMap node geo for {node.Id} is {node.GeoPoint}");
            }
            Dijkstra? dijkstra = new Dijkstra();
            Node startingNode = nodeList.FirstOrDefault(node => node.Id == startNode);
            List<Node> pathNodeList = dijkstra.ExecuteDij(startingNode, nodeList, customerId);//obtain flight path points
            //foreach (Node node in pathNodeList) { System.Console.WriteLine($"Raw Dijk List {node.Id}"); }
            List<Coordinate> pathPoints = geoConnect.NodesToCoordinates(pathNodeList);
            DronePing(pathPoints);//Start drone dummy along the path
            return pathNodeList;
        }
        public String GetPath(int custId)
        {
            Program.GeoConnect geoConnect = InitGeoConnect();

            List<Node> pathNodeList = PathToMap(customerId: custId.ToString());       // creates a flight path to customerId location
            List<(int, string, string, string, string, int)> markerList = geoConnect.GetPathNodesInfo(pathNodeList, custId);  //get the nodes' info to create geoJson map features
            //foreach((int, string, string, string, string, int) marker in markerList){System.Console.WriteLine($"feature ready List marker {marker.Item1}");}
            GeometryFactory? geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            FeatureCollection? featureCollection = new FeatureCollection();
            WKTReader? wktReader = new WKTReader(geometryFactory);
            // creates geoJson features to go with the map
            foreach ((int pointId, string featureType, string shapeType, string pointName, string wkt, int buffer) point in markerList)
            {
                // Read the geography from the WKT
                Geometry? geometry = wktReader.Read(point.wkt);

                // Create a feature with the geometry and an attributes table
                Feature? feature = new Feature(geometry, new AttributesTable());
                feature.Attributes.Add("PointId", point.pointId);
                feature.Attributes.Add("FeatureType", point.featureType);
                System.Console.WriteLine($"Feature list {point.pointName}");
                feature.Attributes.Add("PointName", point.pointName);
                feature.Attributes.Add("Buffer", point.buffer);
                featureCollection.Add(feature);
            }
            GeoJsonWriter? geoJsonWriter = new GeoJsonWriter();
            var geoJsonString = geoJsonWriter.Write(featureCollection);
            return geoJsonString;
        }
    }
}