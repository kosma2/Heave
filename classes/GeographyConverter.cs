using Microsoft.SqlServer.Types;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public static class GeographyConverter
{
    public static string SqlGeographyToGeoJson(SqlGeography sqlGeography)
    {
        try
        {
            // Convert SQLGeography to WKT first
            string wkt = sqlGeography.ToString();

            // Initialize geometry factory with an SRID of 4326 for WGS84
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var wktReader = new WKTReader(geometryFactory);

            // Read the WKT string into a Geometry object
            Geometry geometry = wktReader.Read(wkt);

            // Convert the Geometry to GeoJSON
            var geoJsonWriter = new GeoJsonWriter();
            string geoJson = geoJsonWriter.Write(geometry);

            return geoJson;
        }
        catch (Exception ex)
        {
            // Handle or log the error as needed
            Console.WriteLine("Error converting SqlGeography to GeoJSON: " + ex.Message);
            return null;
        }
    }
}
