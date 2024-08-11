DROP PROCEDURE GetGraphData
GO
CREATE PROCEDURE GetGraphData
    @CustomerId INT
AS
BEGIN
    -- Declare geography point for obstacle
    DECLARE @obstPoint GEOGRAPHY = GEOGRAPHY::STPointFromText('POINT(-117.2904677012464 49.49196670168381)', 4326);
    -- Create buffer around obstacle point
    DECLARE @obst GEOGRAPHY = @obstPoint.STBuffer(300);

    -- Declare variables for Customer's GeoLocation and MarkerName
    DECLARE @CustomerGeo GEOGRAPHY;
    DECLARE @CustomerMarker NVARCHAR(50);

    -- Retrieve the Customer's GeoLocation and MarkerName
    SELECT
        @CustomerGeo = GeoPoint,
        @CustomerMarker = CAST(CustomerId AS NVARCHAR(50))
    FROM
        Customer
    WHERE
        CustomerId = @CustomerId;

    -- Temporary table to hold edges
    CREATE TABLE #Edges
    (
        StartNodeMarkerName NVARCHAR(50),
        EndNodeMarkerName NVARCHAR(50),
        Distance FLOAT
    );

    -- Add the customer node to edge computation using a table variable
    DECLARE @TempNodes TABLE (MarkerName NVARCHAR(50), GeoLocation GEOGRAPHY, ShapeName NVARCHAR(10));
    IF @CustomerGeo IS NOT NULL
    BEGIN
        INSERT INTO @TempNodes (MarkerName, GeoLocation, ShapeName)
        VALUES (@CustomerMarker, @CustomerGeo, 'point');
    END

    -- Insert non-intersecting pairs of points into the temporary edges table, including the temporary customer node
    INSERT INTO #Edges (StartNodeMarkerName, EndNodeMarkerName, Distance)
    SELECT
        A.MarkerName AS StartNodeMarkerName,
        B.MarkerName AS EndNodeMarkerName,
        A.GeoLocation.STDistance(B.GeoLocation) AS Distance
    FROM
        (SELECT * FROM airmarker UNION ALL SELECT * FROM @TempNodes) A,
        (SELECT * FROM airmarker UNION ALL SELECT * FROM @TempNodes) B
    WHERE
        A.MarkerName != B.MarkerName AND
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

    -- Return nodes, including the temporary customer node
    SELECT
        MarkerName,
        GeoLocation.STAsText()
    FROM
        (SELECT * FROM airmarker UNION ALL SELECT * FROM @TempNodes) AS CombinedNodes
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
