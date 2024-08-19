using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;
using Microsoft.AspNetCore.SignalR;

using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;


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
        List<(int, string, string, string, int)> markerList = geoConnect.GetAirMarkers();
        GeometryFactory? geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        FeatureCollection? featureCollection = new FeatureCollection();
        WKTReader? wktReader = new WKTReader(geometryFactory);

        foreach ((int pointId, string shapeType, string pointName, string wkt, int buffer) point in markerList)
        {
            // Read the geography from the WKT
            Geometry? geometry = wktReader.Read(point.wkt);

            // Ensure the geometry is set with the correct SRID
            if (geometry.SRID != 4326)
            {
                geometry.SRID = 4326;
            }

            // Create a feature with the geometry and an attributes table
            Feature? feature = new Feature(geometry, new AttributesTable());
            feature.Attributes.Add("PointId", point.pointId);
            feature.Attributes.Add("PointName", point.pointName);
            feature.Attributes.Add("Buffer", point.buffer);
            featureCollection.Add(feature);

            // Write the feature collection to a GeoJSON string
        }
        GeoJsonWriter? geoJsonWriter = new GeoJsonWriter();
        var geoJsonString = geoJsonWriter.Write(featureCollection);
        return View("Features", geoJsonString);
    }

    [HttpPost]
    public IActionResult CreateFeature(string FeatureName, String Coordinates, int Buffer)
    {
        List<string> coordList = new() { Coordinates };
        Program.GeoConnect geoConnect = InitGeoConnect();
        geoConnect.DBCreateGeoObject("point", FeatureName, coordList, Buffer);
        ViewBag.Message = "Feature Created";
        return View("Confirmation");
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [HttpDelete]
    public IActionResult DeleteFeature(string featureId)
    {
        try
        {
            System.Console.WriteLine($"request delet {featureId}");
            Program.GeoConnect geoConnect = InitGeoConnect();
            geoConnect.DBDeleteAirMarker(Convert.ToInt32(featureId));
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
