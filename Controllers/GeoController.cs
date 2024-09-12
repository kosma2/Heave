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
        Program.GeoConnect geoConnect = InitGeoConnect();
        MapHelper mapHelper = new(_configuration, _hubContext);
        int custId = 1014; //temp id
        List<Node> pathNodeList = mapHelper.PathToMap(customerId: custId.ToString());
        List<(int, string, string, string, string, int)> markerList = geoConnect.GetPathNodesInfo(pathNodeList,custId);  //get the nodes' info to create features
        foreach((int, string, string, string, string, int) marker in markerList){System.Console.WriteLine($"feature ready List marker {marker.Item1}");}
        GeometryFactory? geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        FeatureCollection? featureCollection = new FeatureCollection();
        WKTReader? wktReader = new WKTReader(geometryFactory);

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
        //return View("Features", geoJsonString);
        return View("Dijk", geoJsonString);
    }

    public IActionResult Features()
    {
        Program.GeoConnect geoConnect = InitGeoConnect();
        List<(int, string, string, string, string, int)> markerList = geoConnect.GetAirMarkers();
        GeometryFactory? geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        FeatureCollection? featureCollection = new FeatureCollection();
        WKTReader? wktReader = new WKTReader(geometryFactory);

        foreach ((int pointId, string featureType, string shapeType, string pointName, string wkt, int buffer) point in markerList)
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
            feature.Attributes.Add("FeatureType", point.featureType);
            System.Console.WriteLine(point.featureType);
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
    public IActionResult CreateFeature(string FeatureType,string FeatureName, String Coordinates, int Buffer)
    {
        List<string> coordList = new() { Coordinates };
        Program.GeoConnect geoConnect = InitGeoConnect();
        geoConnect.DBCreateGeoObject(FeatureType,"point", FeatureName, coordList, Buffer);
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
