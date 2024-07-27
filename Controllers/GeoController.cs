using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using Microsoft.AspNetCore.SignalR;


namespace Heave.Controllers;
public class GeoController : Controller
{  
    private readonly IHubContext<CoordinateHub> _hubContext;
    private Program.GeoConnect InitGeoConnect()//handles sql init for adminConnect methods
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        Program.GeoConnect geoConnect = new();
        geoConnect.SqlStr = connectionString;
        return geoConnect;
    }
    private readonly IConfiguration _configuration;
    public GeoController(IConfiguration configuration, IHubContext<CoordinateHub> hubContext)
    {
        _configuration = configuration;
        _hubContext = hubContext;
    }

    static void Reporter_ProgressChanged(object sender, ProgressEventArgs e)
    {
        Console.WriteLine(e.Message); // Handle the progress update
    }


    public static List<Coordinate> GetPointsAlongLine(List<Coordinate> coList,Coordinate start, Coordinate end, double divisor)
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
        List<Coordinate>? points = GetPointsAlongLine(flightPath,start, end, 30); //start, end, number of intervals

        ProgressReporter? reporter = new ProgressReporter(1000, points, _hubContext);
        reporter.ProgressChanged += Reporter_ProgressChanged;
        reporter.Start();
    }
    
    public IActionResult Index()
    {
        return View(new MemberLoginModel());
    }
    public IActionResult Dijkstra()
    {
        Program.GeoConnect geoConnect = InitGeoConnect();
        List<Node> nodeList = geoConnect.DBGetGraphData();

        Dijkstra? dijkstra = new Dijkstra();
        Node startingNode = nodeList.FirstOrDefault(node => node.Id == "n1");
        List<Node> pathNodeList = dijkstra.ExecuteDij(startingNode, nodeList);
        List<Coordinate> pathPoints = geoConnect.NodesToCoordinates(pathNodeList);
        DronePing(pathPoints);//Start drone dummy along the path
        string jsonString = geoConnect.ConvertCoordsToGeoJson(pathPoints);
        return View("Dijk", jsonString);
    }

    public IActionResult Features()
    {
        Program.GeoConnect geoConnect = InitGeoConnect();
        List<(int, string, string, string)> markerList = geoConnect.ShowAirMarkers();
        List<GeoPoint> formatedPoints = new();

        foreach ((int id, string featType, string pointName, string geo) marker in markerList)
        {
            GeoPoint pt = new(marker.pointName, marker.geo);
            formatedPoints.Add(pt);
        }
        string jsnString = geoConnect.ConvertPointsToGeoJson(formatedPoints);

        return View("Features", jsnString);

    }

    [HttpPost]

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
