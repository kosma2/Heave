using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;
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
    
    public IActionResult CreateFeature()
    {
        return View();
    }
    public IActionResult Dijkstra()
    {
        MapHelper mapHelper = new(_configuration, _hubContext);
        string jsonString = mapHelper.PathToMap(customerId: 1012.ToString());
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
    public IActionResult CreateFeature(string FeatureName, String Coordinates, int Buffer)
    {
        List<string> coordList= new(){Coordinates};
        Program.GeoConnect geoConnect = InitGeoConnect();
        geoConnect.DBCreateGeoObject("point",FeatureName,coordList, Buffer);
        ViewBag.Message = "Feature Created";
        return View("Confirmation");
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
