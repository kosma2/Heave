using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;



namespace Heave.Controllers;

public class GeoController : Controller
{
    private Program.GeoConnect InitGeoConnect()//handles sql init for adminConnect methods
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        Program.GeoConnect geoConnect = new();
        geoConnect.SqlStr = connectionString;
        return geoConnect;
    }
    private readonly IConfiguration _configuration;
    public GeoController(IConfiguration configuration)
    {
        _configuration = configuration;
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
            List<GeoPoint> geoPoints = geoConnect.NodesToGeoPoints(pathNodeList);
            
            string jsonString = geoConnect.ConvertToGeoJson(geoPoints);
        return View("Dijk",jsonString);
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
        string jsnString = geoConnect.ConvertToGeoJson(formatedPoints);

        return View("Features",jsnString);

    }

    [HttpPost]

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
