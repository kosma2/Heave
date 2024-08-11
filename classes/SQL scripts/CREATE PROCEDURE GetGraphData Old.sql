CREATE PROCEDURE GetGraphData
AS
BEGIN
    -- Declare geography point for obstacle
    DECLARE @obstPoint GEOGRAPHY = GEOGRAPHY::STPointFromText('POINT(-117.2904677012464 49.49196670168381)', 4326);
    -- Create buffer around obstacle point
    DECLARE @obst GEOGRAPHY = @obstPoint.STBuffer(300);

    -- Temporary table to hold edges
    CREATE TABLE #Edges
    (
        StartNodeMarkerName NVARCHAR(50),
        EndNodeMarkerName NVARCHAR(50),
        Distance FLOAT
    );

    -- Insert non-intersecting pairs of points into the temporary table
    INSERT INTO #Edges (StartNodeMarkerName, EndNodeMarkerName, Distance)
    SELECT
        A.MarkerName AS StartNodeMarkerName,
        B.MarkerName AS EndNodeMarkerName,
        A.GeoLocation.STDistance(B.GeoLocation) AS Distance
    FROM
        airmarker A,
        airmarker B
    WHERE
        A.ID != B.ID AND
        A.ShapeName = 'point' AND
        B.ShapeName = 'point' AND
        ROUND(A.GeoLocation.STDistance(B.GeoLocation), 2) < 1800 AND
        GEOGRAPHY::STLineFromText(
            'LINESTRING(' +
            CAST(A.GeoLocation.Long AS VARCHAR(20)) + ' ' +
            CAST(A.GeoLocation.Lat AS VARCHAR(20)) + ', ' +
            CAST(B.GeoLocation.Long AS VARCHAR(20)) + ' ' +
            CAST(B.GeoLocation.Lat AS VARCHAR(20)) +
            ')', 4326
        ).STIntersects(@obst) = 0;

    -- Return nodes
    SELECT
        ID,
        MarkerName,
        GeoLocation.STAsText()
    FROM
        airmarker
    WHERE
        ShapeName = 'point';

    -- Return edges
    SELECT
        StartNodeMarkerName,
        EndNodeMarkerName,
        Distance
    FROM
        #Edges;

    -- Cleanup: Drop temporary table
    DROP TABLE #Edges;
END;
