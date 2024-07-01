using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;
using Microsoft.SqlServer.Types;


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

    public IActionResult Features()
    {
        Program.GeoConnect geoConnect = InitGeoConnect();
        List<(int, string, string, string)> markerList = geoConnect.ShowAirMarkers();
        List<string> cleanPoints = new();
        foreach (var (ptId, ptType, ptName, geo) in markerList)
        {        
            string wkt = geo;
            SqlGeography sqlGeography = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars(wkt), 4326);
            string geoJson = GeographyConverter.SqlGeographyToGeoJson(sqlGeography);
            cleanPoints.Add(geoJson);
        }

        //return Ok(markerList);
        return View ("Features", cleanPoints);
    }

    [HttpPost]
        
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
